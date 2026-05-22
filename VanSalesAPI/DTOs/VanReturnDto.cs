namespace VanSalesAPI.DTOs
{
    public class VanReturnDto
    {
        public int VanId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
    }
}