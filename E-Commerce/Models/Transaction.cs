namespace E_Commerce.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }

        public string UserEmail { get; set; }
        public string UserRole { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public string ModeOfPayment { get; set; }
        public DateTime DateAndTimeTransaction { get; set; }
        public string ShippingAddress { get; set; }
        public string OrderStatus { get; set; }
        public int? RiderId { get; set; }
        public Rider Rider { get; set; } 

        public ICollection<TransactionItem> Items { get; set; }
    }
}
