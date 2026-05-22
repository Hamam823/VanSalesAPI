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
    [Authorize] // 🔐 أي عملية تحتاج تسجيل دخول
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 🧾 إنشاء فاتورة
        // Admin + Salesman
        // =====================================================
        [Authorize(Roles = "Admin,Salesman")]
        [HttpPost]
        public async Task<ActionResult> CreateInvoice(InvoiceCreateDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 🛡️ تحقق من وجود عناصر
                if (dto.Items == null || !dto.Items.Any())
                    throw new Exception("Invoice items are required");

                // 🛡️ تحقق من نوع الفاتورة
                if (dto.Type != "cash" && dto.Type != "credit")
                    throw new Exception("Invoice type must be cash or credit");

                // 🛡️ تحقق العميل
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == dto.CustomerId);

                if (customer == null)
                    throw new Exception("Customer not found");

                // 🧾 إنشاء الفاتورة
                var invoice = new Invoice
                {
                    CustomerId = dto.CustomerId,
                    Type = dto.Type,
                    Date = DateTime.Now,
                    Total = 0
                };

                _context.Invoices.Add(invoice);

                await _context.SaveChangesAsync();

                decimal total = 0;

                // =====================================================
                // 📦 معالجة المنتجات
                // =====================================================
                foreach (var item in dto.Items)
                {
                    if (item.Qty <= 0)
                        throw new Exception("Quantity must be greater than zero");

                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product == null)
                        throw new Exception($"Product with ID {item.ProductId} not found");

                    if (product.Stock < item.Qty)
                        throw new Exception($"Not enough stock for product: {product.Name}");

                    decimal lineTotal = product.Price * item.Qty;

                    total += lineTotal;

                    var invoiceItem = new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        ProductId = product.Id,
                        Qty = item.Qty,
                        Price = product.Price
                    };

                    _context.InvoiceItems.Add(invoiceItem);

                    // 📉 خصم المخزون
                    product.Stock -= item.Qty;

                    // 📊 حركة مخزون
                    var stockMovement = new StockMovement
                    {
                        ProductId = product.Id,
                        Quantity = item.Qty,
                        MovementType = "OUT",
                        ReferenceType = "Invoice",
                        ReferenceId = invoice.Id,
                        Date = DateTime.Now
                    };

                    _context.StockMovements.Add(stockMovement);
                }

                // 💰 تحديث الإجمالي
                invoice.Total = total;

                // 💳 الفواتير الآجلة
                if (dto.Type == "credit")
                {
                    customer.Balance += total;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Invoice created successfully",
                    invoiceId = invoice.Id,
                    total = total
                });
            }
            catch (Exception ex)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch
                {
                }

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        // =====================================================
        // 📄 جميع الفواتير
        // Admin + Manager
        // =====================================================
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceReadDto>>> GetAll()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.Product)
                .Select(i => new InvoiceReadDto
                {
                    Id = i.Id,
                    Date = i.Date,
                    Type = i.Type,
                    Total = i.Total,
                    CustomerName = i.Customer.Name,

                    Items = i.Items.Select(ii => new InvoiceItemReadDto
                    {
                        ProductName = ii.Product.Name,
                        Qty = ii.Qty,
                        Price = ii.Price
                    }).ToList()
                })
                .ToListAsync();

            return Ok(invoices);
        }

        // =====================================================
        // 📄 فاتورة واحدة
        // Admin + Manager
        // =====================================================
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceReadDto>> GetById(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                    .ThenInclude(ii => ii.Product)
                .Where(i => i.Id == id)
                .Select(i => new InvoiceReadDto
                {
                    Id = i.Id,
                    Date = i.Date,
                    Type = i.Type,
                    Total = i.Total,
                    CustomerName = i.Customer.Name,

                    Items = i.Items.Select(ii => new InvoiceItemReadDto
                    {
                        ProductName = ii.Product.Name,
                        Qty = ii.Qty,
                        Price = ii.Price
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }
    }
}