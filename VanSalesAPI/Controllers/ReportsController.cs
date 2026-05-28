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

        // 📊 Charts
        [HttpGet("charts")]
        public async Task<ActionResult> GetCharts()
        {
            var dailySales = await _context.Invoices
                .GroupBy(i => i.Date.Date)
                .Select(g => new DailySalesDto
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // 🔥 FIX: optimized (no N+1)
            var vanPerformance = await _context.Invoices
                .Where(i => i.VanId != null)
                .GroupBy(i => i.VanId)
                .Select(g => new
                {
                    VanId = g.Key,
                    TotalSales = g.Sum(x => x.Total)
                })
                .Join(_context.Vans,
                    g => g.VanId,
                    v => v.Id,
                    (g, v) => new VanPerformanceDto
                    {
                        VanName = v.Name,
                        TotalSales = g.TotalSales
                    })
                .ToListAsync();

            var productSales = await _context.InvoiceItems
                .Include(i => i.Product)
                .GroupBy(i => i.ProductId)
                .Select(g => new ProductSalesDto
                {
                    ProductName = g.First().Product.Name,
                    Quantity = g.Sum(x => x.Qty)
                })
                .OrderByDescending(x => x.Quantity)
                .ToListAsync();

            var payments = await _context.Invoices
                .GroupBy(i => i.Type)
                .Select(g => new PaymentTypeDto
                {
                    Type = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .ToListAsync();

            var result = new
            {
                dailySales,
                vanPerformance,
                productSales,
                payments
            };

            return Ok(new ApiResponse<object>(
                true,
                "Charts loaded successfully",
                result
            ));
        }

        // 📊 Dashboard
        [HttpGet]
        public async Task<ActionResult> GetDashboard()
        {
            string cacheKey = "dashboard_data";

            if (_cache.TryGetValue(cacheKey, out DashboardDto cachedResult))
            {
                return Ok(new ApiResponse<DashboardDto>(
                    true,
                    "Dashboard loaded from cache",
                    cachedResult
                ));
            }

            var invoices = await _context.Invoices.ToListAsync();

            var totalSales = invoices.Sum(i => i.Total);
            var totalCash = invoices.Where(i => i.Type == "cash").Sum(i => i.Total);
            var totalCredit = invoices.Where(i => i.Type == "credit").Sum(i => i.Total);

            var customersCount = await _context.Customers.CountAsync();
            var productsCount = await _context.Products.CountAsync();
            var vans = await _context.Vans.ToListAsync();

            // 🔥 FIX: optimized grouping
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

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return Ok(new ApiResponse<DashboardDto>(
                true,
                "Dashboard loaded successfully",
                result
            ));
        }
    }
}