namespace TheGadgetHub.Models
{
    public class TechWorldOrders
    {
        public int DistributorOrderId { get; set; }

        public int OrderId { get; set; }

        public String Confirmation { get; set; }

       public List<ODRETechWorld> Items { get; set; } = new List<ODRETechWorld>();
    }

    public class ODRETechWorld
    {
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string Address { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
