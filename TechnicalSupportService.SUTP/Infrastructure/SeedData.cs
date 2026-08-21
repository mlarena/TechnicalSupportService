using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.Constants;
using TechnicalSupportService.Core.Enums;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Infrastructure;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var db = sp.GetRequiredService<ApplicationDbContext>();

        await db.Database.MigrateAsync();

        // Роли
        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new ApplicationRole(roleName));
        }

        // Тестовые пользователи
        var users = new[]
        {
            new { Email = "admin@company.com", Password = "Admin@123", FullName = "Администратор Системы", Role = Roles.Admin, Position = "Системный администратор" },
            new { Email = "engineer@company.com", Password = "Engineer@123", FullName = "Инженер Техподдержки", Role = Roles.Engineer, Position = "Инженер" },
            new { Email = "manager@company.com", Password = "Manager@123", FullName = "Менеджер Проектов", Role = Roles.Manager, Position = "Менеджер" },
            new { Email = "applicant@company.com", Password = "Applicant@123", FullName = "Иван Заявитель", Role = Roles.Applicant, Position = "Сотрудник" }
        };

        foreach (var u in users)
        {
            if (await userManager.FindByEmailAsync(u.Email) != null) continue;
            var user = new ApplicationUser
            {
                UserName = u.Email, Email = u.Email, FullName = u.FullName,
                Position = u.Position, IsActive = true, EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, u.Password);
            if (result.Succeeded) await userManager.AddToRoleAsync(user, u.Role);
        }

        // Отделы
        if (!await db.Departments.AnyAsync())
        {
            db.Departments.AddRange(
                new Department { Name = "IT-отдел", Description = "Информационные технологии" },
                new Department { Name = "Отдел разработки", Description = "Разработка ПО" },
                new Department { Name = "Отдел продаж", Description = "Продажи и маркетинг" }
            );
        }

        // Продукты
        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(
                new Product { Name = "CRM v3.2", ProductType = ProductType.Software, CurrentVersion = "3.2.1", Description = "Система управления клиентами" },
                new Product { Name = "Контроллер Т-100", ProductType = ProductType.Hardware, CurrentVersion = "2.0", Description = "Промышленный контроллер" },
                new Product { Name = "Встраиваемый модуль M1", ProductType = ProductType.Embedded, CurrentVersion = "1.5.3", Description = "Встраиваемый вычислительный модуль" }
            );
        }

        await db.SaveChangesAsync();
    }
}
