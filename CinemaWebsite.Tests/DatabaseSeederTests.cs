using CinemaWebsite.Data;
using CinemaWebsite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CinemaWebsite.Tests;

public class DatabaseSeederTests
{
    [Fact]
    public async Task SeedAsync_AdminCanBeFoundByEmail()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"cinemawebsite-seed-{Guid.NewGuid():N}.db");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        await using (var provider = services.BuildServiceProvider())
        using (var scope = provider.CreateScope())
        {
            await DatabaseSeeder.SeedAsync(scope.ServiceProvider);

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await userManager.FindByEmailAsync("admin@motion.cinema");

            Assert.NotNull(admin);
            Assert.True(await userManager.IsInRoleAsync(admin, "Admin"));
        }

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        File.Delete(dbPath);
    }
}
