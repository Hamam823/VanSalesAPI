namespace VanSalesAPI.DTOs
{
    public class VanProfitDto
    {
        public int VanId { get; set; }
        public string VanName { get; set; }

        public decimal TotalSales { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
        public decimal TotalSalesUSD { get; set; }
        public decimal TotalCostUSD { get; set; }
        public decimal ProfitUSD { get; set; }

        public decimal TotalSalesSYP { get; set; }
        public decimal TotalCostSYP { get; set; }
        public decimal ProfitSYP { get; set; }
    }
}