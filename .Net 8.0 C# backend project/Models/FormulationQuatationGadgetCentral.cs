namespace TheGadgetHub.Models
{
    public class OrderRequestDetailsGadgetCentral
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string Address { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public List<FormulateQuatationGadgetCentral> Items { get; set; } = new List<FormulateQuatationGadgetCentral>();
    }
    public class FormulateQuatationGadgetCentral
    {
        public int FormulateId { get; set; }

        public int OrderId { get; set; }

        public int GId { get; set; }

        public Decimal PricePerUnit { get; set; }

        public int NumberOfUnit { get; set; }

        public String Availability { get; set; }

        public DateTime DeliveryTime { get; set; }
    }
}
