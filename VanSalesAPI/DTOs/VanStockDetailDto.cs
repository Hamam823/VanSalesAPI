namespace VanSalesAPI.DTOs
{
    public class VanStockDetailDto
    {
        public int VanId { get; set; }
        public string VanName { get; set; }

        public List<VanProductStockDto> Products { get; set; } = new();
    }

    public class VanProductStockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int Quantity { get; set; }
    }
}