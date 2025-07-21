using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace E_Commerce.Controllers
{
    public class RiderController : Controller
    {
        private readonly ApplicationDbContext _DB;

        public RiderController(ApplicationDbContext db)
        {
            _DB = db;
        }

        public IActionResult RiderDashboard()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(email) || role != "Rider")
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _DB.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var rider = _DB.Riders.FirstOrDefault(r => r.UserId == user.Id);
            var riderId = rider.RiderId;

            var allBookings = _DB.Transactions
                .Where(t => t.OrderStatus != "Cancelled" && t.OrderStatus != "Declined")
                .ToList();

            var viewModel = new RiderDashboardViewModel
            {
                RiderFullName = rider.FullName,
                TotalBookings = allBookings.Count(b => b.OrderStatus == "Pending"),
                DeliveredCount = allBookings.Count(b => b.OrderStatus == "Delivered" && b.RiderId == riderId),
                PendingCount = allBookings.Count(b => b.OrderStatus == "Accepted" && b.RiderId == riderId),
                AvailableBookings = allBookings.Where(b => b.OrderStatus == "Pending").ToList(),
                PendingDeliveries = allBookings.Where(b => (b.OrderStatus == "Accepted" || b.OrderStatus == "OutForDelivery") && b.RiderId == riderId).ToList()
        };

            return View("RiderDashboard", viewModel);
        }




        private bool IsProfileComplete(Rider rider)
        {
            return !string.IsNullOrEmpty(rider.FullName)
                && !string.IsNullOrEmpty(rider.ContactNumber)
                && !string.IsNullOrEmpty(rider.VehicleInfo);
        }

        public IActionResult Bookings()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Rider")
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = _DB.Transactions
                .Include(t => t.Items)
                .Where(t => t.OrderStatus == "Pending")
                .ToList();

            return View(bookings);
        }

        public async Task<IActionResult> AvailableBookings()
        {
            var pendingBookings = await _DB.Transactions
                .Where(t => t.OrderStatus == "Pending")
                .OrderByDescending(t => t.DateAndTimeTransaction)
                .Take(10)
                .ToListAsync();

            return View(pendingBookings);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptBooking(int transactionId)
        {
            var transaction = await _DB.Transactions.FindAsync(transactionId);
            if (transaction == null)
                return NotFound();

            var riderEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(riderEmail))
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _DB.Users.FirstOrDefaultAsync(u => u.Email == riderEmail);
            if (user == null)
            {
                TempData["Error"] = "Rider not found.";
                return RedirectToAction("AvailableBookings");
            }

            var rider = await _DB.Riders.FirstOrDefaultAsync(r => r.UserId == user.Id);
            if (rider == null)
            {
                TempData["Error"] = "Rider profile not found.";
                return RedirectToAction("AvailableBookings");
            }

            if (!rider.IsProfileComplete)
            {
                TempData["IncompleteProfile"] = true;
                return RedirectToAction("AvailableBookings");
            }

            var existingBooking = await _DB.Transactions
                .FirstOrDefaultAsync(t => t.RiderId == rider.RiderId && t.OrderStatus == "Accepted");

            if (existingBooking != null)
            {
                TempData["Error"] = "You already have an active booking to deliver.";
                return RedirectToAction("AvailableBookings");
            }

            transaction.OrderStatus = "Accepted";
            transaction.RiderId = rider.RiderId;

            await _DB.SaveChangesAsync();

            TempData["Success"] = "Booking accepted successfully.";
            return RedirectToAction("AvailableBookings");
        }


        [HttpGet]
        public IActionResult AcceptedBooking(int? id)
        {
            var riderEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(riderEmail))
                return RedirectToAction("RiderDashboard");

            var user = _DB.Users.FirstOrDefault(u => u.Email == riderEmail);
            if (user == null)
                return RedirectToAction("RiderDashboard");

            var rider = _DB.Riders.FirstOrDefault(r => r.UserId == user.Id);
            if (rider == null)
                return RedirectToAction("RiderDashboard");

            Transaction booking;

            if (id.HasValue)
            {
                booking = _DB.Transactions
                    .Include(t => t.Items)
                    .FirstOrDefault(t => t.TransactionID == id.Value
                                         && t.RiderId == rider.RiderId
                                         && (t.OrderStatus == "Accepted" || t.OrderStatus == "OutForDelivery"));
            }
            else
            {
                booking = _DB.Transactions
                    .Include(t => t.Items)
                    .FirstOrDefault(t => t.RiderId == rider.RiderId
                                         && (t.OrderStatus == "Accepted" || t.OrderStatus == "OutForDelivery"));
            }

            if (booking == null)
            {
                TempData["Info"] = "You have no active bookings.";
                return RedirectToAction("RiderDashboard");
            }

            return View("AcceptedBooking", booking);
        }


        [HttpPost]
        public async Task<IActionResult> MarkOutForDelivery(int id)
        {
            var transaction = await _DB.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            transaction.OrderStatus = "OutForDelivery";
            await _DB.SaveChangesAsync();

            return RedirectToAction("OrderView", new { id = transaction.TransactionID });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsDelivered(int id)
        {
            var transaction = await _DB.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            transaction.OrderStatus = "Delivered";
            await _DB.SaveChangesAsync();

            return RedirectToAction("OrderView", new { id = transaction.TransactionID });
        }

        [HttpGet]
        public IActionResult RiderUpdateInfo()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var user = _DB.Users.FirstOrDefault(u => u.Email == email);
            var rider = _DB.Riders.FirstOrDefault(r => r.UserId == user.Id);

            return View(rider);
        }

        [HttpPost]
        public IActionResult RiderUpdateInfo(Rider updated)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var user = _DB.Users.FirstOrDefault(u => u.Email == email);
            var rider = _DB.Riders.FirstOrDefault(r => r.UserId == user.Id);

            if (rider == null) return NotFound();

            rider.FullName = updated.FullName;
            rider.ContactNumber = updated.ContactNumber;
            rider.VehicleInfo = updated.VehicleInfo;

            _DB.SaveChanges();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("AvailableBookings");
        }


    }
}
