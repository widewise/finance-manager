using FinanceManager.Account.Domain;
using FinanceManager.Events;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Account;

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

    public DbSet<Domain.Account> Accounts { get; set; }
    public DbSet<AccountLimit> AccountLimits { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Currency> Currencies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var foodExpenseId = Guid.Parse("00000000-0000-0000-0000-000000000008");
        var homeExpenseId = Guid.Parse("00000000-0000-0000-0000-000000000009");
        var transportExpenseId = Guid.Parse("00000000-0000-0000-0000-000000000010");
        modelBuilder.Entity<Category>()
            .HasData(
                new Category{ Name = "Transfer", Type = CategoryType.Transfer, Id = TransferConstants.TransferCategoryId, RequestId = TransferConstants.TransferCategoryId.ToString()},
                new Category{ Name = "Salary", Type = CategoryType.Deposit, Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), RequestId = "00000000-0000-0000-0000-000000000002"},
                new Category{ Name = "Bank", Type = CategoryType.Deposit, Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), RequestId = "00000000-0000-0000-0000-000000000003"},
                new Category{ Name = "Return", Type = CategoryType.Deposit, Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), RequestId = "00000000-0000-0000-0000-000000000004"},
                new Category{ Name = "Part time job", Type = CategoryType.Deposit, Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), RequestId = "00000000-0000-0000-0000-000000000005"},
                new Category{ Name = "Rent", Type = CategoryType.Deposit, Id = Guid.Parse("00000000-0000-0000-0000-000000000006"), RequestId = "00000000-0000-0000-0000-000000000006"},
                new Category{ Name = "Other", Type = CategoryType.Deposit, Id = Guid.Parse("00000000-0000-0000-0000-000000000007"), RequestId = "00000000-0000-0000-0000-000000000007"},
                new Category{ Name = "Food", Type = CategoryType.Expense, Id = foodExpenseId, RequestId = "00000000-0000-0000-0000-000000000008"},
                new Category{ Name = "Home", Type = CategoryType.Expense, Id = homeExpenseId, RequestId = "00000000-0000-0000-0000-000000000009"},
                new Category{ Name = "Transport", Type = CategoryType.Expense, Id = transportExpenseId, RequestId = "00000000-0000-0000-0000-000000000010"},
                new Category{ Name = "Personal", Type = CategoryType.Expense, Id = Guid.Parse("00000000-0000-0000-0000-000000000011"), RequestId = "00000000-0000-0000-0000-000000000011"},
                new Category{ Name = "Services", Type = CategoryType.Expense, Id = Guid.Parse("00000000-0000-0000-0000-000000000012"), RequestId = "00000000-0000-0000-0000-000000000012"},
                new Category{ Name = "Rest", Type = CategoryType.Expense, Id = Guid.Parse("00000000-0000-0000-0000-000000000013"), RequestId = "00000000-0000-0000-0000-000000000013"},
                new Category{ Name = "Education", Type = CategoryType.Expense, Id = Guid.Parse("00000000-0000-0000-0000-000000000014"), RequestId = "00000000-0000-0000-0000-000000000014"},
                new Category{ Name = "Beauty", Type = CategoryType.Expense, Id = Guid.Parse("00000000-0000-0000-0000-000000000015"), RequestId = "00000000-0000-0000-0000-000000000015"},
                new Category{ Name = "Low", Type = CategoryType.Expense, Id = Guid.Parse("00000000-0000-0000-0000-000000000016"), RequestId = "00000000-0000-0000-0000-000000000016"},
                new Category{ Name = "Grocery", Type = CategoryType.Expense, ParentId = foodExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000017"), RequestId = "00000000-0000-0000-0000-000000000017"},
                new Category{ Name = "Pub", Type = CategoryType.Expense, ParentId = foodExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000018"), RequestId = "00000000-0000-0000-0000-000000000018"},
                new Category{ Name = "Delivery", Type = CategoryType.Expense, ParentId = foodExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000019"), RequestId = "00000000-0000-0000-0000-000000000019"},
                new Category{ Name = "Restaurant", Type = CategoryType.Expense, ParentId = foodExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000020"), RequestId = "00000000-0000-0000-0000-000000000020"},
                new Category{ Name = "Repair", Type = CategoryType.Expense, ParentId = homeExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000021"), RequestId = "00000000-0000-0000-0000-000000000021"},
                new Category{ Name = "Rent", Type = CategoryType.Expense, ParentId = homeExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000022"), RequestId = "00000000-0000-0000-0000-000000000022"},
                new Category{ Name = "Furniture", Type = CategoryType.Expense, ParentId = homeExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000023"), RequestId = "00000000-0000-0000-0000-000000000023"},
                new Category{ Name = "Electrical appliances", Type = CategoryType.Expense, ParentId = homeExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000024"), RequestId = "00000000-0000-0000-0000-000000000024"},
                new Category{ Name = "Public transport", Type = CategoryType.Expense, ParentId = transportExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000025"), RequestId = "00000000-0000-0000-0000-000000000025"},
                new Category{ Name = "Taxi", Type = CategoryType.Expense, ParentId = transportExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000026"), RequestId = "00000000-0000-0000-0000-000000000026"},
                new Category{ Name = "Airplane", Type = CategoryType.Expense, ParentId = transportExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000027"), RequestId = "00000000-0000-0000-0000-000000000027"},
                new Category{ Name = "Train", Type = CategoryType.Expense, ParentId = transportExpenseId, Id = Guid.Parse("00000000-0000-0000-0000-000000000028"), RequestId = "00000000-0000-0000-0000-000000000028"});
    }
}