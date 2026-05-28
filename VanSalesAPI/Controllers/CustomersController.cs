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
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 📊 كشف حساب العميل
        // =====================================================
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{id}/statement")]
        public async Task<ActionResult> GetStatement(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound(new { message = "Customer not found" });

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

            var transactions = invoices
                .Concat(payments)
                .OrderBy(x => x.Date)
                .ToList();

            decimal balance = 0;

            var items = new List<StatementRowDto>();

            foreach (var t in transactions)
            {
                balance += t.Debit;
                balance -= t.Credit;

                items.Add(new StatementRowDto
                {
                    Date = t.Date,
                    Type = t.Type,
                    Debit = t.Debit,
                    Credit = t.Credit,
                    BalanceAfter = balance
                });
            }

            var result = new CustomerStatementDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                TotalDebit = invoices.Sum(x => x.Debit),
                TotalCredit = payments.Sum(x => x.Credit),
                Balance = balance,
                Items = items
            };

            return Ok(result);
        }

        // =====================================================
        // 📋 جميع العملاء
        // =====================================================
        [Authorize(Roles = "Admin,Manager,Salesman")]
        [HttpGet]
        public async Task<ActionResult> GetAll()
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
        // 👤 عميل واحد
        // =====================================================
        [Authorize(Roles = "Admin,Manager,Salesman")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
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

            return Ok(new
            {
                message = "Customer created successfully",
                customer.Id
            });
        }

        // =====================================================
        // ❌ حذف عميل
        // =====================================================
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound(new { message = "Customer not found" });

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Customer deleted successfully"
            });
        }
    }
}