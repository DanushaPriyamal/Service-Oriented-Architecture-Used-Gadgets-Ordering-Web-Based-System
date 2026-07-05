namespace TheGadgetHub.Models
{
    public class ElectroComOrders
    {
        public int DistributorOrderId { get; set; }

        public int OrderId { get; set; }

        public String Confirmation { get; set; }

        public List<ODRElectroCom> Items { get; set; } = new List<ODRElectroCom>();
    }

    public class ODRElectroCom
    {
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string Address { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
