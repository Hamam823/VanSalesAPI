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
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }

        // 🧾 إنشاء فاتورة
        [Authorize(Roles = "Admin,Salesman")]
        [HttpPost]
        public async Task<ActionResult> CreateInvoice(InvoiceCreateDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (dto.Items == null || !dto.Items.Any())
                    return BadRequest(new ApiResponse<string>(false, "Invoice items are required", null));

                if (dto.Type != "cash" && dto.Type != "credit")
                    return BadRequest(new ApiResponse<string>(false, "Invoice type must be cash or credit", null));

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == dto.CustomerId);

                if (customer == null)
                    return NotFound(new ApiResponse<string>(false, "Customer not found", null));

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

                foreach (var item in dto.Items)
                {
                    if (item.Qty <= 0)
                        return BadRequest(new ApiResponse<string>(false, "Invalid quantity", null));

                    var product = await _context.Products.FindAsync(item.ProductId);

                    if (product == null)
                        return NotFound(new ApiResponse<string>(false, $"Product {item.ProductId} not found", null));

                    if (product.Stock < item.Qty)
                        return BadRequest(new ApiResponse<string>(false, "Not enough stock", null));

                    total += product.Price * item.Qty;

                    _context.InvoiceItems.Add(new InvoiceItem
                    {
                        InvoiceId = invoice.Id,
                        ProductId = product.Id,
                        Qty = item.Qty,
                        Price = product.Price
                    });

                    product.Stock -= item.Qty;

                    _context.StockMovements.Add(new StockMovement
                    {
                        ProductId = product.Id,
                        Quantity = item.Qty,
                        MovementType = "OUT",
                        ReferenceType = "Invoice",
                        ReferenceId = invoice.Id,
                        Date = DateTime.Now
                    });
                }

                invoice.Total = total;

                if (dto.Type == "credit")
                    customer.Balance += total;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new ApiResponse<object>(
                    true,
                    "Invoice created successfully",
                    new { invoice.Id, total }
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return BadRequest(new ApiResponse<string>(
                    false,
                    ex.Message,
                    null
                ));
            }
        }

        // 📄 جميع الفواتير
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<ActionResult> GetAll()
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

            return Ok(new ApiResponse<object>(
                true,
                "Invoices loaded successfully",
                invoices
            ));
        }

        // 📄 فاتورة واحدة
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
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
                return NotFound(new ApiResponse<string>(
                    false,
                    "Invoice not found",
                    null
                ));

            return Ok(new ApiResponse<InvoiceReadDto>(
                true,
                "Invoice loaded successfully",
                invoice
            ));
        }
    }
}