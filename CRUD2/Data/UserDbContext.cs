using CRUD2.Models;
using Microsoft.EntityFrameworkCore;

namespace CRUD2.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ProductModel> Product { get; set; }
        public DbSet<CartModel> Cart { get; set; }
        public DbSet<OrderModel> orders { get; set; }

    }
}
