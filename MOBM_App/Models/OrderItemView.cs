namespace MOBM_App.Models
{
    public class OrderItemView
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Sum => Price * Quantity;
    }
}