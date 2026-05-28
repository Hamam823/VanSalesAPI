using VanSalesAPI.Models;

namespace VanSalesAPI.DTOs
{
    public class PurchaseCreateDto
    {
        public int SupplierId { get; set; }

        public Currency Currency { get; set; }

        public decimal ExchangeRate { get; set; }

        public List<PurchaseItemDto> Items { get; set; }
    }

    public class PurchaseItemDto
    {
        public int ProductId { get; set; }
        public int Qty { get; set; }
        public decimal Cost { get; set; }
    }
}