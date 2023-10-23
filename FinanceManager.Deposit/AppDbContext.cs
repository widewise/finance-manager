using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Deposit;

public class AppDbContext: DbContext
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

    public DbSet<Models.Deposit> Deposits { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
    }
}