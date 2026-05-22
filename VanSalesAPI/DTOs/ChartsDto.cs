namespace VanSalesAPI.DTOs
{
    public class ChartsDto
    {
    }
}
namespace VanSalesAPI.DTOs
{
    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
    }

    public class VanPerformanceDto
    {
        public string VanName { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class ProductSalesDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    public class PaymentTypeDto
    {
        public string Type { get; set; } // Cash / Credit
        public decimal Total { get; set; }
    }
}