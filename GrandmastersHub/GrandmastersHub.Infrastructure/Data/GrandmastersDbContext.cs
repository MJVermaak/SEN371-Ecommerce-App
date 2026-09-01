using System;
using System.Collections.Generic;
using System.Text;
using GrandmastersHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrandmastersHub.Infrastructure.Data
{
    public class GrandmastersDbContext : DbContext
    {
        public GrandmastersDbContext(DbContextOptions<GrandmastersDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}
