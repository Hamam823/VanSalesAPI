namespace VanSalesAPI.DTOs
{
    public class VanProfitDto
    {
        public int VanId { get; set; }
        public string VanName { get; set; }

        public decimal TotalSales { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
    }
}