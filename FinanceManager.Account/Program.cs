using FinanceManager.Account;
using FinanceManager.Account.Consumers;
using FinanceManager.Account.Mapping;
using FinanceManager.Account.Repositories;
using FinanceManager.Account.Services;
using FinanceManager.Events;
using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary;
using FinanceManager.TransportLibrary.Extensions;
using FinanceManager.UnitOfWork.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountLimitRepository, AccountLimitRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IAccountBalanceService, AccountBalanceService>();
builder.Services.AddTransient<IAccountLimitService, AccountLimitService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<ICurrencyService, CurrencyService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (dbConnectionString != null)
{
    // Register database
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(dbConnectionString));
    builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());
    builder.Services.AddEntityFrameworkUnitOfWork();
}

builder.Services.AddApiKeyAuthentication(builder.Configuration);
builder.Services.AddCustomAuthentication(
    builder.Configuration,
    FinanceIdentityConstants.AccountTitle,
    FinanceIdentityConstants.AccountAudience);

builder.Services.AddTransportCore(builder.Configuration);
builder.Services.AddTransportPublisher<NotificationSendEvent>(EventConstants.NotificationExchange);
builder.Services.AddTransportPublisher<ChangeStatisticsEvent>(EventConstants.StatisticsExchange);
builder.Services.AddTransportPublisher<ChangeAccountBalanceRejectEvent>(EventConstants.DepositExchange);

builder.Services.AddTransportConsumerWithReject<ChangeAccountBalanceEvent, ChangeAccountBalanceRejectEvent, ChangeAccountBalanceConsumer>(
    EventConstants.DepositExpenseCommonExchange,
    EventConstants.AccountExchange,
    EventConstants.ChangeAccountBalanceEvent);

builder.Services.AddTransportConsumerWithReject<TransferBetweenAccountsEvent, TransferBetweenAccountsRejectEvent, TransferBetweenAccountsConsumer>(
    EventConstants.TransferExchange,
    EventConstants.AccountExchange,
    EventConstants.TransferBetweenAccountsEvent);

builder.Services.AddScoped<ILinkService, LinkService>();
builder.Services.AddHttpContextAccessor();

IdentityModelEventSource.ShowPII = true;
var app = builder.Build();
app.Services.BuildTransportMap();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("DB connection string: {DBConnectionString}", dbConnectionString);

if (dbConnectionString != null)
{
    logger.LogInformation("Start DB migrating ...");

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    // Here is the migration executed
    dbContext.Database.Migrate();

    logger.LogInformation("DB migrated");
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(a => a.AddDefaultExceptionHandler(logger));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();