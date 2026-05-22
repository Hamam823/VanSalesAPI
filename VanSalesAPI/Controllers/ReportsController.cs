using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanSalesAPI.Data;
using Microsoft.Extensions.Caching.Memory;
using VanSalesAPI.DTOs;

namespace VanSalesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public DashboardController(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }
        [HttpGet("charts")]
        public async Task<ActionResult> GetCharts()
        {
            // 📈 1. المبيعات اليومية
            var dailySales = await _context.Invoices
                .GroupBy(i => i.Date.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // 🚐 2. أداء السيارات
            var vanPerformance = await _context.Vans
                .Select(v => new VanPerformanceDto
                {
                    VanName = v.Name,
                    TotalSales = _context.Invoices
                        .Where(i => i.VanId == v.Id)
                        .Sum(i => i.Total)
                })
                .ToListAsync();

            // 📦 3. أكثر المنتجات مبيعًا
            var productSales = await _context.InvoiceItems
                .GroupBy(i => i.Product.Name)
                .Select(g => new ProductSalesDto
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(x => x.Qty)
                })
                .OrderByDescending(x => x.Quantity)
                .ToListAsync();

            // 💰 4. Cash vs Credit
            var payments = await _context.Invoices
                .GroupBy(i => i.Type)
                .Select(g => new PaymentTypeDto
                {
                    Type = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .ToListAsync();

            return Ok(new
            {
                dailySales,
                vanPerformance,
                productSales,
                payments
            });
        }

        [HttpGet]
        public async Task<ActionResult> GetDashboard()
        {
            string cacheKey = "dashboard_data";

            if (_cache.TryGetValue(cacheKey, out object cachedData))
                return Ok(cachedData);

            // 📦 إجمالي المبيعات (مرة واحدة)
            var invoices = await _context.Invoices.ToListAsync();

            var totalSales = invoices.Sum(i => i.Total);
            var totalCash = invoices.Where(i => i.Type == "cash").Sum(i => i.Total);
            var totalCredit = invoices.Where(i => i.Type == "credit").Sum(i => i.Total);

            // 👥 + 📦 + 🚐 Counts (مرة واحدة)
            var customersCount = await _context.Customers.CountAsync();
            var productsCount = await _context.Products.CountAsync();
            var vans = await _context.Vans.ToListAsync();

            // 🚐 Van Performance (بدون N+1)
            var vanIds = vans.Select(v => v.Id).ToList();

            var vanSales = await _context.Invoices
                .Where(i => i.VanId != null)
                .GroupBy(i => i.VanId)
                .Select(g => new
                {
                    VanId = g.Key,
                    Sales = g.Sum(x => x.Total)
                })
                .ToListAsync();

            var vanStocks = await _context.VanStocks
                .GroupBy(v => v.VanId)
                .Select(g => new
                {
                    VanId = g.Key,
                    Stock = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            var vanSummaries = vans.Select(v =>
            {
                var sales = vanSales.FirstOrDefault(x => x.VanId == v.Id)?.Sales ?? 0;
                var stock = vanStocks.FirstOrDefault(x => x.VanId == v.Id)?.Stock ?? 0;

                return new VanSummaryDto
                {
                    VanId = v.Id,
                    VanName = v.Name,
                    Sales = sales,
                    StockItems = stock
                };
            }).ToList();

            var result = new DashboardDto
            {
                TotalSales = totalSales,
                TotalCash = totalCash,
                TotalCredit = totalCredit,
                CustomersCount = customersCount,
                ProductsCount = productsCount,
                VansCount = vans.Count,
                Vans = vanSummaries
            };

            // ⏱️ Cache لمدة 5 دقائق
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return Ok(result);

        }
    }
}