using VanSalesAPI.Models;

public class Van
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string DriverName { get; set; }

    public string PlateNumber { get; set; }

    public int? SalesmanId { get; set; }
    public Salesman Salesman { get; set; }

    public DateTime CreatedAt { get; set; }
}