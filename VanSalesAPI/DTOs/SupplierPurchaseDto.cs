using VanSalesAPI.DTOs;

public class SupplierPurchaseDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }

    public string Currency { get; set; }

    public decimal TotalOriginal { get; set; }
    public decimal TotalBase { get; set; }

    public List<PurchaseItemDto> Items { get; set; }
}