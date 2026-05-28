using VanSalesAPI.Models;

namespace VanSalesAPI.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string Type { get; set; } // cash / credit

        public decimal Total { get; set; }

        // 👤 العميل
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        // 🚐 إضافة مهمة جدًا
        public int? VanId { get; set; }
        public Van Van { get; set; }
        public Currency Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal TotalOriginal { get; set; } // بالعملة المختارة
        public decimal TotalBase { get; set; }   // USD (أساسي)
        // 📄 تفاصيل الفاتورة
        public List<InvoiceItem> Items { get; set; } = new();
    }
}
