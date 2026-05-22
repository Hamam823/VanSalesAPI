namespace VanSalesAPI.DTOs
{
    public class VanSaleDto
    {
        public int VanId { get; set; }
        public int CustomerId { get; set; }

        public string Type { get; set; } // cash / credit

        public List<VanSaleItemDto> Items { get; set; }
    }

    public class VanSaleItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}