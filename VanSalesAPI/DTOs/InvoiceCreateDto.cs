namespace VanSalesAPI.DTOs
{
    public class InvoiceCreateDto
    {
        public int CustomerId { get; set; }

        public string Type { get; set; } // cash / credit

        public List<InvoiceItemDto> Items { get; set; }
    }

    public class InvoiceItemDto
    {
        public int ProductId { get; set; }

        public int Qty { get; set; }
    }
}