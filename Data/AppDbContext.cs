using Microsoft.EntityFrameworkCore;
using GreenStock.Models;

namespace GreenStock.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentItem> ShipmentItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(
                "Host=localhost;Database=greensstock;Username=postgres;Password=admin"
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Login).HasColumnName("login").IsRequired();
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
                entity.Property(e => e.Role).HasColumnName("role").IsRequired();
                entity.HasIndex(e => e.Login).IsUnique();
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Article).HasColumnName("article").IsRequired();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.Unit).HasColumnName("unit").IsRequired();
                entity.Property(e => e.PurchasePrice).HasColumnName("purchase_price");
                entity.Property(e => e.Stock).HasColumnName("stock");
                entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
                entity.HasIndex(e => e.Article).IsUnique();
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(e => e.CategoryId);
            });

            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.ToTable("shipments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.Recipient).HasColumnName("recipient");
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Shipments)
                      .HasForeignKey(e => e.CreatedBy);
            });

            modelBuilder.Entity<ShipmentItem>(entity =>
            {
                entity.ToTable("shipment_items");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.HasOne(e => e.Shipment)
                      .WithMany(s => s.ShipmentItems)
                      .HasForeignKey(e => e.ShipmentId);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.ShipmentItems)
                      .HasForeignKey(e => e.ProductId);
            });
        }
    }
}
