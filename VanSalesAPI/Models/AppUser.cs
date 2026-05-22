namespace VanSalesAPI.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string Role { get; set; }
        // Admin | Salesman | Manager

        public int? SalesmanId { get; set; }
        public Salesman Salesman { get; set; }
    }
}