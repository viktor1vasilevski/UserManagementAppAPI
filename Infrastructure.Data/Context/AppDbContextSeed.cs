using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Context;

public static class AppDbContextSeed
{
    public static void SeedAdminUser(AppDbContext context, string password)
    {
        context.Database.Migrate();

        if (context.Users.Any(x => x.Role == Role.Admin))
            return;

        var adminUser = User.CreateNew(
            firstName: "Admin",
            lastName: "Admin",
            username: "admin",
            email: "admin@example.com",
            password: password,
            role: Role.Admin,
            isActive: true);

        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}
