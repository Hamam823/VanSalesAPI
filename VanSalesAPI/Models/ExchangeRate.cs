namespace VanSalesAPI.Models
{
    public class ExchangeRate
    {
        public int Id { get; set; }

        public Currency FromCurrency { get; set; }
        public Currency ToCurrency { get; set; }

        public decimal Rate { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}