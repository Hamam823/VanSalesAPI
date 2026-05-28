namespace VanSalesAPI.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Qty { get; set; }

        public decimal Cost { get; set; }
    }
}