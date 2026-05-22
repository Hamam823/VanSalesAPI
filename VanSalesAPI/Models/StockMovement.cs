namespace VanSalesAPI.Models
{
    public class StockMovement
    {
        public int Id { get; set; }

        // 📦 المنتج
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // 🔢 الكمية
        public int Quantity { get; set; }

        // 📊 نوع الحركة: IN / OUT
        public string MovementType { get; set; }

        // 🧾 سبب الحركة (مهم جدًا للتتبع)
        public string? ReferenceType { get; set; }
        // مثال: Invoice / Purchase / Manual

        // 🧾 رقم المرجع (فاتورة مثلاً)
        public int? ReferenceId { get; set; }

        // 📅 التاريخ
        public DateTime Date { get; set; } = DateTime.Now;
    }
}