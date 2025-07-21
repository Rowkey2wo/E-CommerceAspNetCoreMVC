using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;

namespace E_Commerce.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _DB;
        private readonly string _UniquePasswordForAdmins;
        public AdminController(ApplicationDbContext db, IOptions<AdminSettings> options)
        {   
            _DB = db;
            _UniquePasswordForAdmins = options.Value.AdminPassword;
        }

        public IActionResult Dashboard()
        {
            var users = _DB.Users.ToList();
            var products = _DB.Products.ToList();
            var transactions = _DB.Transactions.ToList();

            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var viewModel = new AdminDashboardViewModel
            {
                UserCount = users.Count,
                ProductCount = products.Count,
                OutOfStockCount = products.Count(p => p.Stock == 0),
                LowStockCount = products.Count(p => p.Stock > 0 && p.Stock <= 20),
                RecentUsers = users.OrderByDescending(u => u.Id).ToList(),
                LowStockProducts = products
                    .Where(p => p.Stock <= 20)
                    .OrderBy(p => p.Stock)
                    .Take(3)
                    .ToList(),

                // Top Cards
                CompletedOrderCount = transactions.Count(t => t.OrderStatus == "Delivered"),
                MonthlyRevenue = transactions
                    .Where(t => t.OrderStatus == "Delivered" && t.DateAndTimeTransaction >= startOfMonth)
                    .Sum(t => t.TotalAmount),

                // Order Summary
                OrdersToday = transactions.Count(t => t.DateAndTimeTransaction.Date == today),
                OrdersThisWeek = transactions.Count(t => t.DateAndTimeTransaction >= startOfWeek),
                OrdersThisMonth = transactions.Count(t => t.DateAndTimeTransaction >= startOfMonth)
            };

            return View(viewModel);
        }


        public IActionResult ManageProducts()
        {
            List<ProductModel> products = _DB.Products.OrderBy(p => p.Stock).ToList();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewProduct(ProductModel obj, string adminPassword, IFormFile ImageFile)
        {
            if (adminPassword != _UniquePasswordForAdmins)
            {
                TempData["Error"] = "Invalid password.";
                return RedirectToAction("ManageProducts");
            }

            if (obj == null)
            {
                TempData["Error"] = "Must not be empty";
                return RedirectToAction("ManageProducts");
            }

            // If no image was uploaded, use default
            if (ImageFile == null || ImageFile.Length == 0)
            {
                obj.Image = "/css/img/NoImageAvailable.png";
            }
            else
            {
                // Save uploaded image
                var ext = Path.GetExtension(ImageFile.FileName);
                var uniqueFileName = Path.GetFileNameWithoutExtension(ImageFile.FileName) + "_" + Guid.NewGuid().ToString("N") + ext;
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/css/img", uniqueFileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                obj.Image = "/css/img/" + uniqueFileName;
            }
            _DB.Products.Add(obj);
            await _DB.SaveChangesAsync();

            TempData["Success"] = "Product added successfully.";
            return RedirectToAction("ManageProducts");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productID, int QuantityToAdd, string adminPassword)
        {

            if(adminPassword != _UniquePasswordForAdmins)
            {
                TempData["Error"] = "Invalid password.";
                return RedirectToAction("ManageProducts");
            }

            var product = _DB.Products.FirstOrDefault(p => p.ProductId == productID);

            if (product == null)
            {
                return NotFound();
            }

            if (QuantityToAdd <= 0)
            {
                TempData["Error"] = "Quantity must be greater than 0.";
                return RedirectToAction("ManageProducts", "Admin");
            }

            product.Stock += QuantityToAdd;
            _DB.SaveChanges();

            TempData["Success"] = "Quantity updated successfully!";
            return RedirectToAction("ManageProducts", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(ProductModel obj, IFormFile? ImageFile, string adminPassword)
        {
            if (adminPassword != _UniquePasswordForAdmins)
            {
                TempData["Error"] = "Invalid password.";
                return RedirectToAction("ManageProducts");
            }

            var existingProduct = _DB.Products.FirstOrDefault(p => p.ProductId == obj.ProductId);

            if (existingProduct == null)
            {
                return NotFound();
            }

            
            existingProduct.FoodName = obj.FoodName;
            existingProduct.Category = obj.Category;
            existingProduct.Description = obj.Description;
            existingProduct.Price = obj.Price;

            // Handle image if uploaded
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName);
                var originalName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                var uniqueFileName = originalName + "_" + Guid.NewGuid().ToString("N") + ext;

                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/css/img", uniqueFileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                existingProduct.Image = "/css/img/" + uniqueFileName;
            }

            _DB.SaveChanges();
            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction("ManageProducts");
        }


        [HttpPost]
        public IActionResult DeleteProduct(int ProductId, string adminPassword)
        {
            if (adminPassword != _UniquePasswordForAdmins)
            {
                TempData["Error"] = "Invalid password.";
                return RedirectToAction("ManageProducts");
            }

            var product = _DB.Products.FirstOrDefault(p => p.ProductId == ProductId);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("ManageProducts");
            }

            // Optionally delete image file from wwwroot (if not default)
            if (!string.IsNullOrEmpty(product.Image) && !product.Image.Contains("NoImageAvailable"))
            {
                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.Image.TrimStart('/'));
                if (System.IO.File.Exists(imgPath))
                {
                    System.IO.File.Delete(imgPath);
                }
            }

            _DB.Products.Remove(product);
            _DB.SaveChanges();

            TempData["Success"] = $"Product \"{product.FoodName}\" deleted successfully.";
            return RedirectToAction("ManageProducts");
        }

        [HttpPost]
        public IActionResult AddNewUserUsingAdmin(UserModel obj)
        {
            var confirmPassword = HttpContext.Request.Form["ConfirmPassword"];

            if (_DB.Users.Any(u => u.Email.ToLower() == obj.Email.ToLower()))
            {
                TempData["InvalidInput"] = "This email is already registered.";
                ModelState.AddModelError("Email", "This email is already registered.");
            }

            if (obj.Password != confirmPassword)
            {
                TempData["InvalidInput"] = "Password and confirmation do not match.";
                ModelState.AddModelError("Password", "Password and confirmation do not match.");
            }

            if (!ModelState.IsValid)
            {
                var users = _DB.Users.ToList();
                var products = _DB.Products.ToList();

                var viewModel = new AdminDashboardViewModel
                {
                    UserCount = users.Count,
                    ProductCount = products.Count,
                    OutOfStockCount = products.Count(p => p.Stock == 0),
                    LowStockCount = products.Count(p => p.Stock > 0 && p.Stock <= 20),
                    RecentUsers = users.OrderByDescending(u => u.Id).ToList(),
                    LowStockProducts = products
                        .Where(p => p.Stock <= 20)
                        .OrderBy(p => p.Stock)
                        .Take(3)
                        .ToList()
                };
                return View("Dashboard", viewModel);
            }

            _DB.Users.Add(obj);
            _DB.SaveChanges();

            
            if (obj.Role.ToLower() == "rider")
            {
                Rider newRider = new Rider
                {
                    UserId = obj.Id,
                    FullName = "",
                    ContactNumber = "",
                    VehicleInfo = ""
                };

                _DB.Riders.Add(newRider);
                _DB.SaveChanges();
            }


            TempData["UserSucessfullyAdded"] = "The user has been successfully added to the database.";
            return RedirectToAction("Dashboard", "Admin");
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _DB.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            _DB.Users.Remove(user);
            _DB.SaveChanges();
            TempData["UserSucessfullyAdded"] = $"User '{user.Email}' deleted.";
            return RedirectToAction("Dashboard");
        }





    }
}
