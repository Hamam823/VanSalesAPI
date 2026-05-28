namespace VanSalesAPI.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        public int Qty { get; set; }

        public decimal Price { get; set; }

        // 🧾 الفاتورة
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        public decimal Cost { get; set; }
        // 📦 المنتج
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public decimal UnitPriceBase { get; set; } // USD
    }
}