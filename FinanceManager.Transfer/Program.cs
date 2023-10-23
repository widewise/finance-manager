using FinanceManager.Events;
using FinanceManager.Events.Models;
using FinanceManager.Transfer;
using FinanceManager.Transfer.Consumers;
using FinanceManager.Transfer.Services;
using FinanceManager.TransportLibrary;
using FinanceManager.TransportLibrary.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddTransient<ITransferService, TransferService>();

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
    FinanceIdentityConstants.TransferTitle,
    FinanceIdentityConstants.TransferAudience);

builder.Services.AddTransportCore(builder.Configuration);
builder.Services.AddTransportPublisher<AddDepositEvent>(EventConstants.DepositExchange);
builder.Services.AddTransportPublisher<DeleteDepositEvent>(EventConstants.DepositExchange);
builder.Services.AddTransportPublisher<AddExpenseEvent>(EventConstants.ExpenseExchange);
builder.Services.AddTransportPublisher<DeleteExpenseEvent>(EventConstants.ExpenseExchange);
builder.Services.AddTransportPublisher<TransferBetweenAccountsEvent>(EventConstants.AccountExchange);

builder.Services.AddTransportConsumerWithReject<AddTransferEvent, AddTransferRejectEvent, AddTransferConsumer>(
    EventConstants.FileExchange,
    EventConstants.TransferExchange,
    EventConstants.AddTransferEvent);

builder.Services.AddTransportConsumer<TransferBetweenAccountsRejectEvent, TransferBetweenAccountsRejectConsumer>(
    EventConstants.TransferExchange,
    EventConstants.TransferBetweenAccountsRejectedEvent);

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