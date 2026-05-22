

namespace VanSalesAPI.DTOs
{
    public class InventoryReportDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int WarehouseStock { get; set; }
        public int VanStock { get; set; }
        public int TotalStock { get; set; }
    }
}