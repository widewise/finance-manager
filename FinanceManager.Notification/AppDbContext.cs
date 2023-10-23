using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Notification;

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

    public DbSet<Models.Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Models.Notification>()
            .Property(f => f.Id)
            .ValueGeneratedOnAdd();
    }
}