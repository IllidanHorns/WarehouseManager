using System.Linq;
using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;
using WarehouseManagerApi;

namespace WarehouseManager.Tests.Infrastructure;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("WarehouseManagerTests");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedData(db);
        });
    }

    private static void SeedData(AppDbContext context)
    {
        if (!context.OrderStatuses.Any())
        {
            context.OrderStatuses.AddRange(
                new OrderStatus
                {
                    StatusName = "Создан",
                    IsArchived = false,
                    CreationDatetime = DateTime.UtcNow,
                    UpdateDatetime = DateTime.UtcNow
                },
                new OrderStatus
                {
                    StatusName = "В обработке",
                    IsArchived = false,
                    CreationDatetime = DateTime.UtcNow,
                    UpdateDatetime = DateTime.UtcNow
                });
        }

        if (!context.Roles.Any())
        {
            context.Roles.Add(new Role
            {
                Id = 1,
                RoleName = "Администратор",
                IsArchived = false,
                CreationDatetime = DateTime.UtcNow,
                UpdateDatetime = DateTime.UtcNow
            });
        }

        if (!context.Warehouses.Any())
        {
            context.Warehouses.Add(new Warehouse
            {
                Id = 1,
                Address = "г. Москва, ул. Примерная, д. 1",
                Square = 500,
                CreationDatetime = DateTime.UtcNow,
                UpdateDatetime = DateTime.UtcNow,
                IsArchived = false
            });
        }

        if (!context.Categories.Any())
        {
            context.Categories.Add(new Category
            {
                Id = 1,
                Name = "Электроника",
                Description = "Тестовая категория",
                CreationDatetime = DateTime.UtcNow,
                UpdateDatetime = DateTime.UtcNow,
                IsArchived = false
            });
        }

        if (!context.Products.Any())
        {
            context.Products.Add(new Product
            {
                Id = 1,
                ProductName = "Клавиатура",
                Price = 100,
                Weight = 1,
                CategoryId = 1,
                CreationDatetime = DateTime.UtcNow,
                UpdateDatetime = DateTime.UtcNow,
                IsArchived = false
            });
        }

        if (!context.Users.Any())
        {
            context.Users.Add(new User
            {
                Id = 1,
                Email = "admin@example.com",
                FirstName = "Админ",
                MiddleName = "Системы",
                PasswordHash = "test",
                PhoneNumber = "+10000000000",
                RoleId = 1,
                IsArchived = false,
                CreationDatetime = DateTime.UtcNow
            });
        }

        if (!context.Remaining.Any())
        {
            context.Remaining.Add(new Remaining
            {
                Id = 1,
                ProductId = 1,
                WarehouseId = 1,
                Quantity = 15,
                CreationDatetime = DateTime.UtcNow,
                UpdateDatetime = DateTime.UtcNow,
                IsArchived = false
            });
        }

        context.SaveChanges();
    }
}

