namespace VanSalesAPI.DTOs
{
    public class DashboardDto
    {
        public decimal TotalSales { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalCash { get; set; }

        public int CustomersCount { get; set; }
        public int ProductsCount { get; set; }
        public int VansCount { get; set; }

        public List<VanSummaryDto> Vans { get; set; }
    }

    public class VanSummaryDto
    {
        public int VanId { get; set; }
        public string VanName { get; set; }

        public decimal Sales { get; set; }
        public int StockItems { get; set; }
    }
}