namespace VanSalesAPI.Models
{
    public class Salesman
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public List<Van> Vans { get; set; }
    }
}