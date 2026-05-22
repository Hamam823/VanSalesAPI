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
    [Authorize] // 🔐 أي مستخدم لازم يكون مسجل دخول
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 📊 كشف حساب العميل
        // Admin + Manager فقط
        // =====================================================
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{id}/statement")]
        public async Task<ActionResult<CustomerStatementDto>> GetStatement(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound("Customer not found");

            // =========================
            // 📦 الفواتير (Debit)
            // =========================
            var invoices = await _context.Invoices
                .Where(i => i.CustomerId == id)
                .Select(i => new StatementRowDto
                {
                    Date = i.Date,
                    Type = "Invoice",
                    Debit = i.Total,
                    Credit = 0,
                    BalanceAfter = 0
                })
                .ToListAsync();

            // =========================
            // 💰 الدفعات (Credit)
            // =========================
            var payments = await _context.Payments
                .Where(p => p.CustomerId == id)
                .Select(p => new StatementRowDto
                {
                    Date = p.Date,
                    Type = "Payment",
                    Debit = 0,
                    Credit = p.Amount,
                    BalanceAfter = 0
                })
                .ToListAsync();

            // =========================
            // 🔗 دمج العمليات
            // =========================
            var transactions = invoices
                .Concat(payments)
                .OrderBy(x => x.Date)
                .ToList();

            // =========================
            // 🧠 حساب الرصيد المتحرك
            // =========================
            decimal balance = 0;

            var statement = new List<StatementRowDto>();

            foreach (var t in transactions)
            {
                balance += t.Debit;
                balance -= t.Credit;

                statement.Add(new StatementRowDto
                {
                    Date = t.Date,
                    Type = t.Type,
                    Debit = t.Debit,
                    Credit = t.Credit,
                    BalanceAfter = balance
                });
            }

            // =========================
            // 📊 النتيجة النهائية
            // =========================
            var result = new CustomerStatementDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                TotalDebit = invoices.Sum(x => x.Debit),
                TotalCredit = payments.Sum(x => x.Credit),
                Balance = balance,
                Items = statement
            };

            return Ok(result);
        }

        // =====================================================
        // 📋 عرض جميع العملاء
        // جميع المستخدمين المصرح لهم
        // =====================================================
        [Authorize(Roles = "Admin,Manager,Salesman")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone
                })
                .ToListAsync();

            return Ok(customers);
        }

        // =====================================================
        // 👤 تفاصيل عميل
        // =====================================================
        [Authorize(Roles = "Admin,Manager,Salesman")]
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetById(int id)
        {
            var customer = await _context.Customers
                .Where(c => c.Id == id)
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone
                })
                .FirstOrDefaultAsync();

            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        // =====================================================
        // ➕ إنشاء عميل
        // =====================================================
        [Authorize(Roles = "Admin,Manager,Salesman")]
        [HttpPost]
        public async Task<ActionResult> Create(CustomerDto dto)
        {
            var customer = new Customer
            {
                Name = dto.Name,
                Phone = dto.Phone
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(customer);
        }

        // =====================================================
        // ❌ حذف عميل
        // Admin فقط
        // =====================================================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Customer deleted successfully"
            });
        }
    }
}