namespace E_Commerce.Models
{
    public class TransactionItem
    {
        public int TransactionItemID { get; set; }

        public int TransactionID { get; set; }

        
        public Transaction Transaction { get; set; }

        public string FoodName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
