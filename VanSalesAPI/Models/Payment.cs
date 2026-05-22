namespace VanSalesAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        // 💰 قيمة الدفعة
        public decimal Amount { get; set; }

        // 📅 التاريخ
        public DateTime Date { get; set; }

        // 📝 ملاحظة
        public string? Notes { get; set; }

        // 🔗 العميل
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }
    }
}