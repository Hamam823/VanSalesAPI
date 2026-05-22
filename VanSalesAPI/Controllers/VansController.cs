using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanSalesAPI.Data;
using VanSalesAPI.DTOs;
using VanSalesAPI.Models;

namespace VanSalesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔐 حماية كاملة
    public class VansController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VansController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 📦 تحميل السيارة
        // Admin + Salesman
        // =====================================================
        [Authorize(Roles = "Admin,Salesman")]
        [HttpPost("load")]
        public async Task<ActionResult> LoadVan(LoadVanDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var van = await _context.Vans.FindAsync(dto.VanId);
                if (van == null)
                    return BadRequest("Van not found");

                var product = await _context.Products.FindAsync(dto.ProductId);
                if (product == null)
                    return BadRequest("Product not found");

                if (product.Stock < dto.Quantity)
                    return BadRequest("Insufficient warehouse stock");

                product.Stock -= dto.Quantity;

                var vanStock = await _context.VanStocks
                    .FirstOrDefaultAsync(vs => vs.VanId == dto.VanId && vs.ProductId == dto.ProductId);

                if (vanStock == null)
                {
                    vanStock = new VanStock
                    {
                        VanId = dto.VanId,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity
                    };
                    _context.VanStocks.Add(vanStock);
                }
                else
                {
                    vanStock.Quantity += dto.Quantity;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Van loaded successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        // =====================================================
        // 🚐 إنشاء سيارة
        // Admin + Manager
        // =====================================================
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<ActionResult> CreateVan(VanDto dto)
        {
            var van = new Van
            {
                Name = dto.Name,
                DriverName = dto.DriverName,
                PlateNumber = dto.PlateNumber,
                CreatedAt = DateTime.Now
            };

            _context.Vans.Add(van);
            await _context.SaveChangesAsync();

            return Ok(van);
        }

        // =====================================================
        // 📊 Inventory Report
        // Admin + Manager
        // =====================================================
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("inventory-report")]
        public async Task<ActionResult> GetInventoryReport()
        {
            var products = await _context.Products.ToListAsync();

            var vanStocks = await _context.VanStocks
                .GroupBy(v => v.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            var result = products.Select(p =>
            {
                var vs = vanStocks.FirstOrDefault(x => x.ProductId == p.Id);

                return new InventoryReportDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    WarehouseStock = p.Stock,
                    VanStock = vs?.Quantity ?? 0,
                    TotalStock = p.Stock + (vs?.Quantity ?? 0)
                };
            });

            return Ok(result);
        }

        // =====================================================
        // 🚐 تفاصيل مخزون السيارات
        // =====================================================
        [HttpGet("stock-detail")]
        public async Task<ActionResult> GetVanStockDetail()
        {
            var vans = await _context.Vans.ToListAsync();

            var vanStocks = await _context.VanStocks
                .Include(v => v.Product)
                .ToListAsync();

            var result = vans.Select(van => new VanStockDetailDto
            {
                VanId = van.Id,
                VanName = van.Name,
                Products = vanStocks
                    .Where(vs => vs.VanId == van.Id)
                    .Select(vs => new VanProductStockDto
                    {
                        ProductId = vs.ProductId,
                        ProductName = vs.Product.Name,
                        Quantity = vs.Quantity
                    }).ToList()
            });

            return Ok(result);
        }

        // =====================================================
        // 📊 Van Summary
        // =====================================================
        [HttpGet("{id}/summary")]
        public async Task<ActionResult> GetVanSummary(int id)
        {
            var stock = await _context.VanStocks
                .Where(v => v.VanId == id)
                .SumAsync(x => x.Quantity);

            var sales = await _context.Invoices
                .Where(i => i.VanId == id)
                .SumAsync(i => i.Total);

            return Ok(new
            {
                VanId = id,
                TotalStock = stock,
                TotalSales = sales
            });
        }

        // =====================================================
        // 🔁 إرجاع للمستودع
        // =====================================================
        [Authorize(Roles = "Admin,Salesman")]
        [HttpPost("return")]
        public async Task<ActionResult> ReturnToWarehouse(VanReturnDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var vanStock = await _context.VanStocks
                    .FirstOrDefaultAsync(vs => vs.VanId == dto.VanId && vs.ProductId == dto.ProductId);

                if (vanStock == null || vanStock.Quantity < dto.Quantity)
                    return BadRequest("Invalid stock");

                vanStock.Quantity -= dto.Quantity;

                var product = await _context.Products.FindAsync(dto.ProductId);
                product.Stock += dto.Quantity;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Returned successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        // =====================================================
        // 💰 ربح السيارات
        // =====================================================
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("profit")]
        public async Task<ActionResult> GetVanProfit()
        {
            var result = await _context.InvoiceItems
                .Where(i => i.Invoice.VanId != null)
                .GroupBy(i => i.Invoice.VanId)
                .Select(g => new VanProfitDto
                {
                    VanId = g.Key.Value,
                    VanName = _context.Vans
                        .Where(v => v.Id == g.Key)
                        .Select(v => v.Name)
                        .FirstOrDefault(),

                    TotalSales = g.Sum(x => x.Qty * x.Price),
                    TotalCost = g.Sum(x => x.Qty * x.Product.Cost),
                    Profit = g.Sum(x => x.Qty * x.Price) - g.Sum(x => x.Qty * x.Product.Cost)
                })
                .ToListAsync();

            return Ok(result);
        }

        // =====================================================
        // 🚐 البيع من السيارة
        // =====================================================
        [Authorize(Roles = "Admin,Salesman")]
        [HttpPost("sell")]
        public async Task<ActionResult> SellFromVan(VanSaleDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var van = await _context.Vans.FindAsync(dto.VanId);
                if (van == null)
                    return BadRequest("Van not found");

                var invoice = new Invoice
                {
                    CustomerId = dto.CustomerId,
                    VanId = dto.VanId,
                    Date = DateTime.Now,
                    Type = dto.Type,
                    Total = 0
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                decimal total = 0;

                foreach (var item in dto.Items)
                {
                    var vanStock = await _context.VanStocks
                        .FirstOrDefaultAsync(vs => vs.VanId == dto.VanId && vs.ProductId == item.ProductId);

                    if (vanStock == null || vanStock.Quantity < item.Quantity)
                        return BadRequest("Insufficient van stock");

                    var product = await _context.Products.FindAsync(item.ProductId);

                    total += product.Price * item.Quantity;

                    vanStock.Quantity -= item.Quantity;

                    _context.InvoiceItems.Add(new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        ProductId = product.Id,
                        Qty = item.Quantity,
                        Price = product.Price
                    });
                }

                invoice.Total = total;

                if (dto.Type == "credit")
                {
                    var customer = await _context.Customers.FindAsync(dto.CustomerId);
                    customer.Balance += total;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { invoice.Id, total });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }
    }
}