using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace E_Commerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _DB;
        private readonly string _UniquePasswordForAdmins;
        public AccountController(ApplicationDbContext DB, IOptions<AdminSettings> options)
        {
            _DB = DB;
            _UniquePasswordForAdmins = options.Value.AdminPassword;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult ProcessLogin(UserModel user)
        {
            if(ModelState.IsValid)
            {
                var loggedInUser = _DB.Users
                    .FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);


                if (loggedInUser != null)
                {
                    
                    //session the logged in user
                    HttpContext.Session.SetString("UserEmail", loggedInUser.Email);
                    HttpContext.Session.SetString("UserRole", loggedInUser.Role);
                    HttpContext.Session.SetString("UserAddress", loggedInUser.Address ?? "");

                    if (loggedInUser.Role == "Admin")
                    {
                        return RedirectToAction("Dashboard","Admin");
                    }
                    if (loggedInUser.Role == "Rider")
                    {
                        return RedirectToAction("RiderDashboard", "Rider");
                    }
                    return RedirectToAction("OrderPage","Home");
                }else{

                    ModelState.AddModelError(string.Empty, "Invalid email or password.");

                }
            }
            return View("Login", user);
        }
        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var user = _DB.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            return View(user);
        }
        public IActionResult Register()
        {
            return View();
        }

        public IActionResult ProcessRegister(UserModel obj)
        {
            var confirmPassword = HttpContext.Request.Form["ConfirmPassword"];

            if (_DB.Users.Any(u => u.Email.ToLower() == obj.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
            }

            if (obj.Password != confirmPassword)
            {
                ModelState.AddModelError("Password", "Password and confirmation do not match.");
            }

            if (!ModelState.IsValid)
            {
                return View("Register", obj);
            }

            obj.Role = "User";

            _DB.Users.Add(obj);
            _DB.SaveChanges();

            TempData["Success"] = "Registration successful. Please login.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        [HttpPost]
        public IActionResult UpdateUser(UserModel updateUser, string adminPassword)
        {
            var currentPassword = HttpContext.Request.Form["CurrentPassword"];
            var newPassword = HttpContext.Request.Form["NewPassword"];
            var confirmPassword = HttpContext.Request.Form["ConfirmPassword"];

            if (adminPassword != _UniquePasswordForAdmins)
            {
                TempData["ErrorUpdateUser"] = "Invalid Security password.";
                return RedirectToAction("UpdateUser");
            }

            var currentUser = _DB.Users.FirstOrDefault(u => u.Id == updateUser.Id);

            if (currentUser == null)
            {
                TempData["ErrorUpdateUser"] = "User not found.";
                return RedirectToAction("UpdateUser");
            }

            if (_DB.Users.Any(u => u.Email.ToLower() == updateUser.Email.ToLower() && u.Id != updateUser.Id))
            {
                TempData["ErrorUpdateUser"] = "This email is already registered by another user.";
                return RedirectToAction("UpdateUser", new { id = updateUser.Id });
            }


            if (currentUser.Password != currentPassword)
            {
                TempData["ErrorUpdateUser"] = "Current password is incorrect.";
                return RedirectToAction("UpdateUser", updateUser);
            }

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (newPassword != confirmPassword)
                {
                    TempData["ErrorUpdateUser"] = "New passwords do not match.";
                    return RedirectToAction("UpdateUser", new { id = updateUser.Id });
                }

                currentUser.Password = newPassword;
            }

            currentUser.Email = updateUser.Email;

            // Update Role only if admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Admin")
            {
                currentUser.Role = updateUser.Role;
            }

            // Update Password
            currentUser.Password = newPassword;

            _DB.SaveChanges();

            TempData["SuccessUserUpdate"] = "User updated successfully!";
            return RedirectToAction("Dashboard", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateProfile(UserModel updatedUser, string CurrentPassword, string? NewPassword, string? ConfirmNewPassword)
        {
            
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "User is not logged in.";
                return RedirectToAction("Login");
            }

            
            var existingUser = _DB.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            
            if (existingUser.Password != CurrentPassword)
            {
                TempData["Error"] = "Incorrect current password.";
                return RedirectToAction("Profile");
            }

            
            if (string.IsNullOrWhiteSpace(updatedUser.Address))
            {
                TempData["Error"] = "Address is required.";
                return RedirectToAction("Profile");
            }
            existingUser.Address = updatedUser.Address;

            
            if (!string.IsNullOrEmpty(NewPassword) || !string.IsNullOrEmpty(ConfirmNewPassword))
            {
                if (NewPassword != ConfirmNewPassword)
                {
                    TempData["Error"] = "New passwords do not match.";
                    return RedirectToAction("Profile");
                }

                existingUser.Password = NewPassword;
            }

            // 6. Save changes
            _DB.SaveChanges();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("OrderPage","Home");
        }

    }
}
