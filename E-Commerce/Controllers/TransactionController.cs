using E_Commerce.Data;
using E_Commerce.Models.DTO;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _DB;

        public TransactionController(ApplicationDbContext db)
        {
            _DB = db;
        }

        public IActionResult TransactionIndex()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["Error"] = "Please log in to view your transactions.";
                return RedirectToAction("Login", "Account");
            }

            var userTransactions = _DB.Transactions
                .Where(t => t.UserEmail == userEmail)
                .OrderByDescending(t => t.DateAndTimeTransaction)
                .ToList();

            return View(userTransactions);
        }

        [HttpPost]
        public IActionResult Checkout([FromBody] CheckoutDTO checkout)
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                var user = _DB.Users.FirstOrDefault(u => u.Email == userEmail);

                if (user == null)
                {
                    return Json(new { message = "User not found. Please login again." });
                }

                if (string.IsNullOrEmpty(user.Address))
                {
                    return Json(new { message = "Missing address. Please update your profile before checking out." });
                }

                if (checkout == null || checkout.Items == null || checkout.Items.Count == 0)
                {
                    return Json(new { message = "Cart is empty." });
                }

                // Check and update stock
                foreach (var item in checkout.Items)
                {
                    var product = _DB.Products.FirstOrDefault(p => p.ProductId == item.Id);
                    if (product == null)
                    {
                        return Json(new { message = $"Product with ID {item.Id} not found." });
                    }

                    if (product.Stock < item.Quantity)
                    {
                        return Json(new { message = $"Not enough stock for {product.FoodName}. Available: {product.Stock}" });
                    }

                    product.Stock -= item.Quantity;
                }

                // Create transaction
                var transaction = new Transaction
                {
                    UserEmail = user.Email,
                    UserRole = user.Role,
                    TotalAmount = checkout.TotalAmount,
                    Discount = checkout.Discount,
                    DateAndTimeTransaction = DateTime.Now,
                    ShippingAddress = user.Address,
                    OrderStatus = "Pending",
                    ModeOfPayment = checkout.ModeOfPayment,
                    Items = checkout.Items.Select(i => new TransactionItem
                    {
                        FoodName = i.Name,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                };

                _DB.Transactions.Add(transaction);
                _DB.SaveChanges();

                return Json(new { redirectUrl = Url.Action("TransactionIndex", "Transaction") });
            }
            catch (Exception ex)
            {
                return Json(new { message = "An error occurred while processing the order: " + ex.Message });
            }
        }

        public IActionResult OrderView(int id)
        {
            var transaction = _DB.Transactions
                .Include(t => t.Items)
                .FirstOrDefault(t => t.TransactionID == id);

            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        public IActionResult History()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userRole))
            {
                TempData["Error"] = "Unauthorized access. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            List<Transaction> history;

            if (userRole == "Admin")
            {
                history = _DB.Transactions
                    .Include(t => t.Items)
                    .Include(t => t.Rider)
                    .ThenInclude(r => r.User)
                    .Where(t => t.OrderStatus == "Delivered")
                    .OrderByDescending(t => t.DateAndTimeTransaction)
                    .ToList();
            }
            else if (userRole == "Rider")
            {
                var user = _DB.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                {
                    TempData["Error"] = "Rider account not found.";
                    return RedirectToAction("Login", "Account");
                }

                var rider = _DB.Riders.FirstOrDefault(r => r.UserId == user.Id);
                if (rider == null)
                {
                    TempData["Error"] = "Rider profile not found.";
                    return RedirectToAction("Login", "Account");
                }

                history = _DB.Transactions
                    .Include(t => t.Items)
                    .Where(t => t.OrderStatus == "Delivered" && t.RiderId == rider.RiderId)
                    .OrderByDescending(t => t.DateAndTimeTransaction)
                    .ToList();
            }
            else
            {
                return RedirectToAction("TransactionIndex");
            }

            return View(history);
        }




    }
}
