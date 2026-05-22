namespace VanSalesAPI.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }
        public decimal Balance { get; set; } = 0;
        // علاقة: عميل لديه فواتير كثيرة
        public List<Invoice>? Invoices { get; set; }
        public List<Payment> Payments { get; set; } = new();
    }
}