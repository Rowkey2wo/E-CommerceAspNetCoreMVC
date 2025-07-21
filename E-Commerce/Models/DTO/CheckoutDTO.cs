namespace E_Commerce.Models.DTO
{
    public class CheckoutDTO
    {
        public List<CartItemDTO> Items { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalAmount { get; set; }

        public string ModeOfPayment { get; set; }

        public string Address { get; set; } // optional, can be removed if not used
    }

    public class CartItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
