namespace E_Commerce.Models
{
    public class RiderDashboardViewModel
    {
        public string RiderFullName { get; set; }
        public int TotalBookings { get; set; }
        public int DeliveredCount { get; set; }
        public int PendingCount { get; set; }

        public List<Transaction> AvailableBookings { get; set; }
        public List<Transaction> PendingDeliveries { get; set; }

    }

    public class Booking
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

}
