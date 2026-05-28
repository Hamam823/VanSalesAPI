using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanSalesAPI.Data;
using VanSalesAPI.DTOs;
using VanSalesAPI.Models;

namespace VanSalesAPI.Controllers
{
    [ApiController]
    [Route("api/mobile")]
    [Authorize(Roles = "Salesman")]
    public class MobileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MobileController(AppDbContext context)
        {
            _context = context;
        }

        // 📦 مخزون السيارة
        [HttpGet("van-stock/{vanId}")]
        public async Task<IActionResult> GetVanStock(int vanId)
        {
            var van = await _context.Vans.FindAsync(vanId);

            if (van == null)
                return NotFound(new ApiResponse<string>(
                    false,
                    "Van not found",
                    null
                ));

            var stock = await _context.VanStocks
                .Where(v => v.VanId == vanId)
                .Include(v => v.Product)
                .Select(v => new
                {
                    v.ProductId,
                    ProductName = v.Product.Name,
                    Price = v.Product.Price,
                    v.Quantity
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>(
                true,
                "Van stock loaded successfully",
                new
                {
                    VanId = van.Id,
                    VanName = van.Name,
                    Items = stock
                }
            ));
        }

        // 📊 ملخص يومي
        [HttpGet("daily-summary/{vanId}")]
        public async Task<IActionResult> GetDailySummary(int vanId)
        {
            var today = DateTime.Today;

            var invoices = await _context.Invoices
                .Where(i => i.VanId == vanId && i.Date.Date == today)
                .ToListAsync();

            var totalSales = invoices.Sum(i => i.Total);
            var totalCash = invoices.Where(i => i.Type == "cash").Sum(i => i.Total);
            var totalCredit = invoices.Where(i => i.Type == "credit").Sum(i => i.Total);
            var invoicesCount = invoices.Count;

            return Ok(new ApiResponse<object>(
                true,
                "Daily summary loaded",
                new
                {
                    Date = today,
                    TotalSales = totalSales,
                    TotalCash = totalCash,
                    TotalCredit = totalCredit,
                    InvoicesCount = invoicesCount
                }
            ));
        }

        // 📄 آخر الفواتير
        [HttpGet("recent-sales/{vanId}")]
        public async Task<IActionResult> GetRecentSales(int vanId)
        {
            var sales = await _context.Invoices
                .Where(i => i.VanId == vanId)
                .Include(i => i.Customer)
                .OrderByDescending(i => i.Date)
                .Take(20)
                .Select(i => new
                {
                    i.Id,
                    i.Date,
                    i.Total,
                    i.Type,
                    Customer = i.Customer != null ? i.Customer.Name : null
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>(
                true,
                "Recent sales loaded",
                sales
            ));
        }
    }
}