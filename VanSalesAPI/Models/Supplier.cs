namespace VanSalesAPI.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Phone { get; set; }

        public string Address { get; set; }

        // 💰 حساب المورد
        public decimal BalanceUSD { get; set; }
        public decimal BalanceSYP { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Purchase> Purchases { get; set; } = new();
    }
}