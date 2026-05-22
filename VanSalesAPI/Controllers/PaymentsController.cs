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
    [Authorize] // 🔐 تسجيل دخول مطلوب
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 💰 إضافة دفعة
        // Admin + Salesman
        // =====================================================
        [Authorize(Roles = "Admin,Salesman")]
        [HttpPost]
        public async Task<ActionResult> AddPayment(PaymentCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 🛡️ تحقق المبلغ
                if (dto.Amount <= 0)
                    return BadRequest("Amount must be greater than zero");

                // 🛡️ تحقق العميل
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == dto.CustomerId);

                if (customer == null)
                    return BadRequest("Customer not found");

                // =====================================================
                // 💰 إنشاء الدفعة
                // =====================================================
                var payment = new Payment
                {
                    CustomerId = dto.CustomerId,
                    Amount = dto.Amount,
                    Date = DateTime.Now,
                    Notes = dto.Notes
                };

                _context.Payments.Add(payment);

                // =====================================================
                // 💳 تخفيض الدين
                // =====================================================
                customer.Balance -= dto.Amount;

                // 🛡️ منع الرصيد السالب
                if (customer.Balance < 0)
                    customer.Balance = 0;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Payment added successfully",
                    customerBalance = customer.Balance
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
        // 📊 كشف الحساب
        // Admin + Manager
        // =====================================================
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("statement/{customerId}")]
        public async Task<ActionResult> GetStatement(int customerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null)
                return NotFound("Customer not found");

            // =====================================================
            // 📦 الفواتير
            // =====================================================
            var invoices = await _context.Invoices
                .Where(i => i.CustomerId == customerId)
                .Select(i => new
                {
                    Date = i.Date,
                    Type = "Invoice",
                    Debit = i.Total,
                    Credit = 0m
                })
                .ToListAsync();

            // =====================================================
            // 💰 الدفعات
            // =====================================================
            var payments = await _context.Payments
                .Where(p => p.CustomerId == customerId)
                .Select(p => new
                {
                    Date = p.Date,
                    Type = "Payment",
                    Debit = 0m,
                    Credit = p.Amount
                })
                .ToListAsync();

            // =====================================================
            // 🔗 دمج العمليات
            // =====================================================
            var transactions = invoices
                .Concat(payments)
                .OrderBy(x => x.Date)
                .ToList();

            decimal balance = 0;

            var items = transactions.Select(x =>
            {
                balance += x.Debit;
                balance -= x.Credit;

                return new StatementRowDto
                {
                    Date = x.Date,
                    Type = x.Type,
                    Debit = x.Debit,
                    Credit = x.Credit,
                    BalanceAfter = balance
                };
            }).ToList();

            // =====================================================
            // 📊 النتيجة النهائية
            // =====================================================
            return Ok(new CustomerStatementDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                TotalDebit = invoices.Sum(x => x.Debit),
                TotalCredit = payments.Sum(x => x.Credit),
                Balance = balance,
                Items = items
            });
        }
    }
}