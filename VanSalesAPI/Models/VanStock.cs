namespace VanSalesAPI.Models
{
    public class VanStock
    {
        public int Id { get; set; }

        public int VanId { get; set; }
        public Van Van { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }
    }
}