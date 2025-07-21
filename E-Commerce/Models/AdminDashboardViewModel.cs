namespace E_Commerce.Models
{
    public class AdminDashboardViewModel
    {
        public int UserCount { get; set; }
        public int ProductCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int LowStockCount { get; set; }
        public int CompletedOrderCount { get; set; }
        public decimal MonthlyRevenue { get; set; }

        public int OrdersToday { get; set; }
        public int OrdersThisWeek { get; set; }
        public int OrdersThisMonth { get; set; }


        public List<UserModel> RecentUsers { get; set; } = new();
        public List<ProductModel> LowStockProducts { get; set; } = new();
    }
}
