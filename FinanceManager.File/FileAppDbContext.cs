using Microsoft.EntityFrameworkCore;

namespace FinanceManager.File;

public class FileAppDbContext: DbContext
{
    private readonly IConfiguration _configuration;

    public FileAppDbContext(
        IConfiguration configuration,
        DbContextOptions<FileAppDbContext> options) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));

    public DbSet<Models.ImportSession> ImportSessions { get; set; } = null!;
    public DbSet<Models.ExportSession> ExportSessions { get; set; } = null!;
    public DbSet<Models.File> Files { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Models.ImportSession>()
            .Property(f => f.Id)
            .ValueGeneratedOnAdd();
        builder.Entity<Models.ExportSession>()
            .Property(f => f.Id)
            .ValueGeneratedOnAdd();
        builder.Entity<Models.File>()
            .Property(f => f.Id)
            .ValueGeneratedOnAdd();
    }
}