namespace VanSalesAPI.DTOs
{
    public class InvoiceReadDto
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Type { get; set; }

        public decimal Total { get; set; }

        public string CustomerName { get; set; }

        public List<InvoiceItemReadDto> Items { get; set; }
    }

    public class InvoiceItemReadDto
    {
        public string ProductName { get; set; }

        public int Qty { get; set; }

        public decimal Price { get; set; }

        public decimal Total => Qty * Price;
    }
}