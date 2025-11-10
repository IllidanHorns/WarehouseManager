using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using WarehouseManager.Core.ViewDTOs;
using WarehouseManager.Core.Models;
using WWarehouseManager.Core.ViewDTOs;

namespace WarehouseManager.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderStatus> OrderStatuses { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Warehouse> Warehouses { get; set; } = null!;
        public DbSet<Remaining> Remaining { get; set; } = null!;
        public DbSet<PriceHistory> PriceHistories { get; set; } = null!;
        public DbSet<OrdersProducts> OrdersProducts { get; set; } = null!;
        public DbSet<OperationsAudit> OperationsAudits { get; set; } = null!;
        public DbSet<EmployeesWarehouses> EmployeesWarehouses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>(entity =>
            {
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(x => x.Name).HasMaxLength(255).IsRequired();
                entity.HasIndex(x => x.Name).IsUnique();
                entity.Property(x => x.Description).HasMaxLength(1000);
            });

            builder.Entity<Product>(entity =>
            {
                entity.ToTable(tb => tb.HasTrigger("TR_Product_InsertPriceHistory"));
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(x => x.ProductName).HasMaxLength(255).IsRequired();
                entity.HasIndex(x => x.ProductName).IsUnique();
                entity.Property(x => x.Price).HasPrecision(10, 2).IsRequired();
                entity.Property(x => x.Weight).HasPrecision(8, 2).IsRequired();

                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Warehouse>(entity =>
            {
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(x => x.Address).HasMaxLength(1000).IsRequired();
                entity.HasIndex(x => x.Address).IsUnique();
                entity.Property(x => x.Square).IsRequired();
            });

            builder.Entity<Role>(entity =>
            {
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(x => x.RoleName).HasMaxLength(255).IsRequired();
                entity.HasIndex(x => x.RoleName).IsUnique();
            });

            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
                entity.HasIndex(x => x.Email).IsUnique();
                entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
                entity.HasIndex(x => x.PasswordHash).IsUnique();
                entity.Property(x => x.FirstName).HasMaxLength(255).IsRequired();
                entity.Property(x => x.MiddleName).HasMaxLength(255).IsRequired();
                entity.Property(x => x.Patronymic).HasMaxLength(255);
                entity.Property(x => x.PhoneNumber).HasMaxLength(255).IsRequired();
                entity.HasIndex(x => x.PhoneNumber).IsUnique();

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(x => x.Salary).HasPrecision(10, 2).IsRequired();

                entity.HasOne(e => e.User)
                      .WithOne(u => u.Employee)
                      .HasForeignKey<Employee>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.Orders)
                    .WithOne(o => o.Employee)
                    .HasForeignKey(o => o.EmployeeId);  
            });

            builder.Entity<OrderStatus>(entity =>
            {
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(x => x.StatusName).HasMaxLength(255).IsRequired();
                entity.HasIndex(x => x.StatusName).IsUnique();
            });

            builder.Entity<Order>(entity =>
            {
                entity.ToTable(tb => tb.HasTrigger("TR_Orders_AuditChanges"));
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(x => x.TotalPrice).HasPrecision(18, 2);

                entity.HasOne(o => o.Warehouse)
                      .WithMany(w => w.Orders)
                      .HasForeignKey(o => o.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.OrderStatus)
                      .WithMany(os => os.Orders)
                      .HasForeignKey(o => o.StatusId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Employee)
                      .WithMany(e => e.Orders)
                      .HasForeignKey(o => o.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);
            });


            builder.Entity<OrdersProducts>(entity =>
            {
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.HasKey(op => op.Id);

                entity.Property(x => x.OrderPrice).HasPrecision(10, 2).IsRequired();
                entity.Property(x => x.Quantity).IsRequired();
                entity.Property(x => x.TotalPrice).HasPrecision(18, 2).IsRequired();

                entity.HasOne(op => op.Order)
                      .WithMany(o => o.OrdersProducts)
                      .HasForeignKey(op => op.OrderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(op => op.Product)
                      .WithMany(p => p.OrdersProducts)
                      .HasForeignKey(op => op.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PriceHistory>(entity =>
            {
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(x => x.OldPrice).HasPrecision(10, 2).IsRequired();
                entity.Property(x => x.NewPrice).HasPrecision(10, 2).IsRequired();

                entity.HasOne(ph => ph.Product)
                      .WithMany(p => p.PriceHistories)
                      .HasForeignKey(ph => ph.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Remaining>(entity =>
            {
                entity.ToTable(tb => tb.HasTrigger("TR_Remaining_CheckQuantity"));
                entity.Property(e => e.CreationDatetime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.Property(x => x.Quantity).IsRequired();

                entity.Property(x => x.ProductId).IsRequired();

                entity.HasOne(r => r.Product)
                      .WithMany(p => p.Remainings)
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Warehouse)
                      .WithMany(w => w.Remaining)
                      .HasForeignKey(r => r.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OperationsAudit>(entity =>
            {
                entity.HasOne(oa => oa.User)
                      .WithMany()
                      .HasForeignKey(oa => oa.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<EmployeesWarehouses>(entity =>
            {
                entity.Property(e => e.CreationDateTime).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsArchived).HasDefaultValue(false);
                entity.HasKey(ew => ew.Id);

                entity.HasOne(ew => ew.Employee)

                      .WithMany(e => e.EmployeesWarehouses)
                      .HasForeignKey(ew => ew.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ew => ew.Warehouse)
                      .WithMany(w => w.EmployeesWarehouses)
                      .HasForeignKey(ew => ew.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<WarehouseStockDto>()
                .ToView("WarehouseStockDistribution")
                .HasNoKey();

            builder.Entity<OrderStatusDistributionDto>()
                .ToView("OrderStatusDistribution")
                .HasNoKey();

            builder.Entity<CategoryProductCountDto>()
                .ToView("CategoryProductCount")
                .HasNoKey();

            builder.Entity<MonthlyRevenueTrendDto>()
                .ToView(null)
                .HasNoKey();

            builder.Entity<TopCategoryRevenueDto>()
                .ToView(null)
                .HasNoKey();

            builder.Entity<WarehouseOrderStatsDto>()
                .ToView(null)
                .HasNoKey();

            builder.Entity<EmployeePerformanceDto>()
                .ToView(null)
                .HasNoKey();
        }
    }
}
