namespace VanSalesAPI.DTOs
{
    public class PaymentCreateDto
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}