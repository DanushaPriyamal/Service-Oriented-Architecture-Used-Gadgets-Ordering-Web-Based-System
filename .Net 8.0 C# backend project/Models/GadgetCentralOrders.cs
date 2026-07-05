namespace TheGadgetHub.Models
{
    public class GadgetCentralOrders
    {
        public int DistributorOrderId { get; set; }

        public int OrderId { get; set; }

        public String Confirmation { get; set; }

        public List<ODREGadgetCentral> Items { get; set; } = new List<ODREGadgetCentral>();
    }

    public class ODREGadgetCentral
    {
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string Address { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
