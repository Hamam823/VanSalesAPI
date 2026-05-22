namespace VanSalesAPI.DTOs
{
    public class CustomerStatementDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public decimal TotalDebit { get; set; }   // فواتير
        public decimal TotalCredit { get; set; }  // دفعات
        public decimal Balance { get; set; }
        

        public List<StatementRowDto> Items { get; set; } = new();
    }

    public class StatementRowDto
    {
        public DateTime Date { get; set; }

        public string Type { get; set; } // Invoice / Payment

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public decimal BalanceAfter { get; set; }
    }
}