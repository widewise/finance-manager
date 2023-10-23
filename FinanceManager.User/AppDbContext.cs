using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.User;

public class AppDbContext : IdentityUserContext<Models.User, long>
{
    private readonly IConfiguration _configuration;

    public AppDbContext(
        IConfiguration configuration,
        DbContextOptions<AppDbContext> options) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
}