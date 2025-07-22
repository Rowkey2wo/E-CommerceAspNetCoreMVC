using E_Commerce.Models;

namespace E_Commerce.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Seed Admin User
            if (!context.Users.Any())
            {
                context.Users.Add(new UserModel
                {
                    Email = "admin@example.com",
                    Password = "123",
                    Role = "Admin",
                    Address = "Admin Office"
                });
            }

            // Seed Products
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new ProductModel { FoodName = "Spring Rolls", Category = "Appetizer", Description = "Crispy vegetable spring rolls served with sweet chili sauce.", Price = 120, Image = "/css/img/SpringRolls.png", Stock = 24 },
                    new ProductModel { FoodName = "Garlic Bread", Category = "Appetizer", Description = "Toasted bread slices with garlic butter and herbs.", Price = 90, Image = "/css/img/GarlicBread.png", Stock = 21 },
                    new ProductModel { FoodName = "Mozzarella Sticks", Category = "Appetizer", Description = "Golden fried mozzarella sticks served with marinara sauce.", Price = 110, Image = "/css/img/MozzarellaSticks.png", Stock = 20 },
                    new ProductModel { FoodName = "Chicken Adobo", Category = "Lunch", Description = "Classic Filipino chicken adobo simmered in soy sauce, vinegar, and garlic.", Price = 180, Image = "/css/img/ChickenAdobo.png", Stock = 30 },
                    new ProductModel { FoodName = "Beef Tapa", Category = "Lunch", Description = "Tender marinated beef strips served with garlic rice and egg.", Price = 200, Image = "/css/img/BeefTapa.png", Stock = 28 },
                    new ProductModel { FoodName = "Chicken Teriyaki", Category = "Lunch", Description = "Grilled chicken glazed in teriyaki sauce served over steamed rice.", Price = 210, Image = "/css/img/ChickenTeriyakiRice.png", Stock = 25 },
                    new ProductModel { FoodName = "Grilled Salmon", Category = "Dinner", Description = "Grilled salmon fillet served with vegetables and lemon butter sauce.", Price = 250, Image = "/css/img/GrilledSalmon.png", Stock = 24 },
                    new ProductModel { FoodName = "Pasta Alfredo", Category = "Dinner", Description = "Creamy fettuccine pasta with white sauce and mushrooms.", Price = 190, Image = "/css/img/PastaAlfredo.png", Stock = 26 },
                    new ProductModel { FoodName = "Sirloin Steak", Category = "Dinner", Description = "Juicy sirloin steak grilled to perfection, served with mashed potatoes and vegetables.", Price = 320, Image = "/css/img/SirloinSteak.png", Stock = 23 },
                    new ProductModel { FoodName = "Easy Cheesy Beef", Category = "Dinner", Description = "Baked sweet potato loaded with cheesy beef chilli and sour cream topping.", Price = 240, Image = "/css/img/EasyCheesyBeefCasserole.png", Stock = 21 }
                );
            }

            context.SaveChanges();
        }
    }
}
