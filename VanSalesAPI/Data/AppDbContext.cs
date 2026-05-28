using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VanSalesAPI.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace VanSalesAPI.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>

    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options
        ) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Van> Vans { get; set; }
       
        public DbSet<Salesman> Salesmen { get; set; }
        public DbSet<VanStock> VanStocks { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Customer>()
                .Property(c => c.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Payment>()
       .Property(p => p.Amount)
       .HasPrecision(18, 2);
        }
    }
}