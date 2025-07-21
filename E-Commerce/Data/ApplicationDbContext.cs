using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<Rider> Riders { get; set; }

        public DbSet<ProductModel> Products { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionItem> TransactionsItem { get; set; }

        //Steps for the table
        //1. Create a model class
        //2. Add DB Set
        //3. Add migration like AddEcommerceUsersTable
        //4. update database

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<UserModel>().HasData(
        //        new UserModel
        //        {
        //            Id = 1,
        //            Email = "admin123@gmail.com",
        //            Password = "123",
        //            Role = "Admin"
        //        },
        //        new UserModel
        //        {
        //            Id = 2,
        //            Email = "yer123@gmail.com",
        //            Password = "123",
        //            Role = "User"
        //        });
        //}

    }
}
