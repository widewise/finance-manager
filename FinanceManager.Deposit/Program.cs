using FinanceManager.Deposit;
using FinanceManager.Deposit.Consumers;
using FinanceManager.Deposit.Services;
using FinanceManager.Events;
using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary;
using FinanceManager.TransportLibrary.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddTransient<IDepositService, DepositService>();

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
}

builder.Services.AddApiKeyAuthentication(builder.Configuration);
builder.Services.AddCustomAuthentication(
    builder.Configuration,
    FinanceIdentityConstants.DepositTitle,
    FinanceIdentityConstants.DepositAudience);

builder.Services.AddTransportCore(builder.Configuration);
builder.Services.AddTransportPublisher<AddDepositRejectEvent>(EventConstants.FileExchange);
builder.Services.AddTransportPublisher<ChangeAccountBalanceEvent>(EventConstants.AccountExchange);

builder.Services.AddTransportConsumerWithReject<AddDepositEvent, AddDepositRejectEvent, AddDepositConsumer>(
    EventConstants.FileExchange,
    EventConstants.DepositExchange,
    EventConstants.AddDepositEvent);

builder.Services.AddTransportConsumer<DeleteDepositEvent, DeleteDepositConsumer>(
    EventConstants.DepositExchange,
    EventConstants.DeleteDepositEvent);

builder.Services.AddTransportConsumer<ChangeAccountBalanceRejectEvent, ChangeAccountBalanceRejectConsumer>(
    EventConstants.DepositExpenseCommonExchange,
    EventConstants.DepositChangeAccountBalanceRejectedEvent);

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