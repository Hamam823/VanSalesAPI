namespace VanSalesAPI.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        public Currency Currency { get; set; }

        public decimal ExchangeRate { get; set; }

        public decimal TotalOriginal { get; set; } // SYP أو USD
        public decimal TotalBase { get; set; }     // USD

        public List<PurchaseItem> Items { get; set; } = new();
    }
}