using E_Commerce.Data;
using E_Commerce.Models;

namespace E_Commerce.Data
{
    public static class DbInitializer
    {
        public static void SeedAdmin(ApplicationDbContext context)
        {
            if (!context.Users.Any(u => u.Role == "Admin"))
            {
                var adminUser = new UserModel
                {
                    Email = "admin@example.com",
                    Password = "123", 
                    Role = "Admin",
                    Address = "Admin HQ"
                };

                context.Users.Add(adminUser);
                context.SaveChanges();
            }
        }
    }
}
