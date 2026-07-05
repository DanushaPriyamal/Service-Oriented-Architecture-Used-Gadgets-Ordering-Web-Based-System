namespace TheGadgetHub.Models
{
    public class Gadget
    {
        public int GId { get; set; }

        public String GName { get; set; }

        public String? GDescription { get; set; }

        public Decimal GPrice { get; set; }

        public String? GImagePath { get; set; }

        public int GStatus { get; set; }
    }
}

