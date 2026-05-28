using VanSalesAPI.Models;

namespace VanSalesAPI.DTOs
{
    public class InvoiceCreateDto
    {
        public int CustomerId { get; set; }

        public string Type { get; set; } // cash / credit
        public Currency Currency { get; set; }  // USD or SYP

        public decimal ExchangeRate { get; set; } // إذا SYP

        public List<InvoiceItemDto> Items { get; set; }
    }

    public class InvoiceItemDto
    {
        public int ProductId { get; set; }

        public int Qty { get; set; }
    }
}