namespace TheGadgetHub.Models
{
    public class OrderRequest
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string Address { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }

    public class CartItem
    {
        public int GId { get; set; }      // Matches gadget.gId
        public int Quantity { get; set; }
        public decimal GPrice { get; set; } // Matches gadget.gPrice
    }
}
