using Microsoft.AspNetCore.Identity;

namespace VanSalesAPI.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }

        // علاقة اختيارية فقط
        public int? SalesmanId { get; set; }
        public Salesman? Salesman { get; set; }
    }
}