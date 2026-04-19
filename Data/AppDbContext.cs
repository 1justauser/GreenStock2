using GreenStock.Logging;
using GreenStock.Models;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GreenStock.Data;

/// <summary>
/// Контекст базы данных GreenStock.
/// Поддерживает два режима создания:
/// — через DI (передаётся <see cref="DbContextOptions{TContext}"/>);
/// — напрямую из форм (используется соединение из <see cref="DbConfig.ConnectionString"/>).
/// </summary>
public class AppDbContext : DbContext
{
    private static readonly ILogger _log = AppLogger.For<AppDbContext>();

    /// <summary>
    /// Таблица пользователей.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Таблица категорий.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Таблица товаров.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Таблица отгрузок.
    /// </summary>
    public DbSet<Shipment> Shipments => Set<Shipment>();

    /// <summary>
    /// Таблица позиций отгрузок.
    /// </summary>
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();

    /// <summary>
    /// Инициализирует контекст с явными параметрами (используется при DI / тестах).
    /// </summary>
    /// <param name="options">Параметры подключения EF Core.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// Инициализирует контекст с параметрами по умолчанию из <see cref="DbConfig.ConnectionString"/>.
    /// Используется формами напрямую: <c>using var db = new AppDbContext();</c>
    /// </summary>
    public AppDbContext() { }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            if (DbConfig.UseInMemory)
            {
                _log.Debug("AppDbContext: InMemory-режим (тесты)");
                optionsBuilder.UseInMemoryDatabase(DbConfig.InMemoryDbName);
            }
            else
            {
                _log.Debug("AppDbContext: конфигурация из DbConfig.ConnectionString");
                optionsBuilder.UseNpgsql(DbConfig.ConnectionString);
            }
        }
    }

    /// <summary>
    /// Настройка модели базы данных.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users").HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnType("uuid").ValueGeneratedOnAdd();

            // UserRole enum хранится как строка в колонке role
            e.Property(u => u.Role)
             .HasConversion<string>()
             .HasColumnName("role");

            e.HasMany(u => u.Shipments)
             .WithOne(s => s.CreatedByUser)
             .HasForeignKey(s => s.CreatedBy);
        });

        // ── Category ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories").HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnType("uuid").ValueGeneratedOnAdd();

            e.HasMany(c => c.Products)
             .WithOne(p => p.Category)
             .HasForeignKey(p => p.CategoryId);
        });

        // ── Product ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("products").HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnType("uuid").ValueGeneratedOnAdd();
            e.Property(p => p.CategoryId).HasColumnType("uuid");
            e.Property(p => p.ExpiryDate).HasColumnName("expiry_date").IsRequired(false);
            e.Property(p => p.SellingPrice).HasColumnName("selling_price").HasDefaultValue(0m);
        });

        // ── Shipment ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Shipment>(e =>
        {
            e.ToTable("shipments").HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnType("uuid").ValueGeneratedOnAdd();
            e.Property(s => s.CreatedBy).HasColumnType("uuid");
            e.Property(s => s.Recipient).HasColumnName("recipient").HasMaxLength(200);

            e.HasMany(s => s.Items)
             .WithOne(i => i.Shipment)
             .HasForeignKey(i => i.ShipmentId);
        });

        // ── ShipmentItem ──────────────────────────────────────────────────────
        modelBuilder.Entity<ShipmentItem>(e =>
        {
            e.ToTable("shipment_items").HasKey(i => i.Id);
            e.Property(i => i.Id).HasColumnType("uuid").ValueGeneratedOnAdd();
            e.Property(i => i.ShipmentId).HasColumnType("uuid");
            e.Property(i => i.ProductId).HasColumnType("uuid");
            e.Property(i => i.Price).HasColumnName("price").HasDefaultValue(0m);
        });
    }
}
