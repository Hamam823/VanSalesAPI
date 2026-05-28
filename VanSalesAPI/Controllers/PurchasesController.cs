using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanSalesAPI.Data;
using VanSalesAPI.DTOs;
using VanSalesAPI.Models;

namespace VanSalesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PurchasesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddSupplier(SupplierCreateDto dto)
        {
            var supplier = new Supplier
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address,
                BalanceUSD = 0,
                BalanceSYP = 0,
                CreatedAt = DateTime.Now
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Supplier created successfully",
                supplierId = supplier.Id
            });
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> CreatePurchase(PurchaseCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var supplier = await _context.Suppliers.FindAsync(dto.SupplierId);

                if (supplier == null)
                    return BadRequest("Supplier not found");

                var purchase = new Purchase
                {
                    SupplierId = dto.SupplierId,
                    Currency = dto.Currency,
                    ExchangeRate = dto.ExchangeRate,
                    Date = DateTime.Now
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                decimal totalOriginal = 0;

                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);

                    if (product == null)
                        return BadRequest("Product not found");

                    // 📦 stock update
                    product.Stock += item.Qty;

                    // 💰 update cost
                    product.Cost = item.Cost;

                    totalOriginal += item.Qty * item.Cost;

                    _context.PurchaseItems.Add(new PurchaseItem
                    {
                        PurchaseId = purchase.Id,
                        ProductId = item.ProductId,
                        Qty = item.Qty,
                        Cost = item.Cost
                    });
                }

                // 💱 currency calculation
                purchase.TotalOriginal = totalOriginal;

                purchase.TotalBase = dto.Currency == Currency.USD
                    ? totalOriginal
                    : totalOriginal / dto.ExchangeRate;

                // 💳 FIXED supplier balance (BASE ONLY)
                supplier.BalanceUSD += purchase.TotalBase;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Purchase created successfully",
                    purchaseId = purchase.Id,
                    totalUSD = purchase.TotalBase,
                    totalOriginal = purchase.TotalOriginal
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("pay")]
        public async Task<IActionResult> Pay(int supplierId, decimal amount, Currency currency, decimal exchangeRate = 1)
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);

            if (supplier == null)
                return NotFound("Supplier not found");

            decimal amountBase;

            // 💱 تحويل إلى USD (Base Currency)
            if (currency == Currency.USD)
            {
                amountBase = amount;
                supplier.BalanceUSD -= amountBase;
            }
            else
            {
                amountBase = amount / exchangeRate;
                supplier.BalanceSYP -= amount;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment done successfully",
                paidOriginal = amount,
                currency = currency.ToString(),
                exchangeRate,
                paidBaseUSD = amountBase,
                supplier.BalanceUSD,
                supplier.BalanceSYP
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Purchases)
                    .ThenInclude(p => p.Items)
                        .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
                return NotFound();

            var result = new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Phone = supplier.Phone,
                Address = supplier.Address,
                BalanceUSD = supplier.BalanceUSD,
                BalanceSYP = supplier.BalanceSYP,

                Purchases = supplier.Purchases.Select(p => new SupplierPurchaseDto
                {
                    Id = p.Id,
                    Date = p.Date,
                    Currency = p.Currency.ToString(),
                    TotalOriginal = p.TotalOriginal,
                    TotalBase = p.TotalBase,

                    Items = p.Items.Select(i => new PurchaseItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        Qty = i.Qty,
                        Cost = i.Cost
                    }).ToList()
                }).ToList()
            };

            return Ok(result);
        }

    }
}