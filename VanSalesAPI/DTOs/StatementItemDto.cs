namespace VanSalesAPI.DTOs
{
    public class StatementItemDto
    {
        public DateTime Date { get; set; }

        // Invoice / Payment
        public string Type { get; set; }

        // 📊 مدين (فواتير)
        public decimal Debit { get; set; }

        // 💰 دائن (دفعات)
        public decimal Credit { get; set; }

        // 🧾 وصف العملية
        public string Description { get; set; }

        // 💡 الرصيد بعد العملية (اختياري لكن مهم)
        public decimal BalanceAfter { get; set; }
    }
}