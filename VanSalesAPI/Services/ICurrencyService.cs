namespace VanSalesAPI.Services
{
    public interface ICurrencyService
    {
        decimal ConvertToBase(decimal amount, decimal rate);
    }

    public class CurrencyService : ICurrencyService
    {
        public decimal ConvertToBase(decimal amount, decimal rate)
        {
            return amount / rate;
        }
    }
}