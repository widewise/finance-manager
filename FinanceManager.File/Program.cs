using FinanceManager.Events;
using FinanceManager.Events.Models;
using FinanceManager.File;
using FinanceManager.File.Consumers;
using FinanceManager.File.Models;
using FinanceManager.File.Services;
using FinanceManager.TransportLibrary;
using FinanceManager.TransportLibrary.Extensions;
using FinanceManager.TransportLibrary.Models;
using Microsoft.EntityFrameworkCore;
using RedLockNet;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.Configure<FinanceManagerSettings>(
    builder.Configuration.GetSection(FinanceManagerSettings.Section));
builder.Services.Configure<ImportDataSettings>(
    builder.Configuration.GetSection(ImportDataSettings.Section));
builder.Services.Configure<ApiKeySettings>(
    builder.Configuration.GetSection(ApiKeySettings.Section));
builder.Services.AddSingleton<IDistributedLockFactory, GlobalLockService>();
builder.Services.AddScoped<IFinanceManagerRestClient, FinanceManagerRestClient>();
builder.Services.AddScoped<IImportCurrenciesService, ImportCurrenciesService>();
builder.Services.AddScoped<IImportCategoriesService, ImportCategoriesService>();
builder.Services.AddScoped<IImportAccountsService, ImportAccountsService>();
builder.Services.AddScoped<IImportMoneyTransactionsService, ImportMoneyTransactionsService>();
builder.Services.AddTransient<IImportSessionService, ImportSessionService>();
builder.Services.AddTransient<IExportSessionService, ExportSessionService>();
builder.Services.AddTransient<ISessionFileSerializer, SessionFileSerializer>();
builder.Services.AddTransient<IImportDataService, ImportDataService>();
builder.Services.AddTransient<IExportDataService, ExportDataService>();
builder.Services.AddHostedService<ImportDataBackgroundService>();
builder.Services.AddHostedService<ExportDataBackgroundService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (dbConnectionString != null)
{
    // Register database
    builder.Services.AddDbContext<FileAppDbContext>(options =>
        options.UseNpgsql(dbConnectionString));
}

builder.Services.AddCustomAuthentication(
    builder.Configuration,
    FinanceIdentityConstants.FileTitle,
    FinanceIdentityConstants.FileAudience);

builder.Services.AddTransportCore(builder.Configuration);

builder.Services.AddTransportPublisher<AddDepositEvent>(EventConstants.DepositExchange);
builder.Services.AddTransportPublisher<AddExpenseEvent>(EventConstants.ExpenseExchange);
builder.Services.AddTransportPublisher<AddTransferEvent>(EventConstants.TransferExchange);

builder.Services.AddTransportConsumer<AddDepositRejectEvent, AddDepositRejectConsumer>(
    EventConstants.FileExchange,
    EventConstants.AddDepositRejectedEvent);

builder.Services.AddTransportConsumer<AddExpenseRejectEvent, AddExpenseRejectConsumer>(
    EventConstants.FileExchange,
    EventConstants.AddExpenseRejectedEvent);

builder.Services.AddTransportConsumer<AddTransferRejectEvent, AddTransferRejectConsumer>(
    EventConstants.FileExchange,
    EventConstants.AddTransferRejectedEvent);

var app = builder.Build();
app.Services.BuildTransportMap();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("DB connection string: {DBConnectionString}", dbConnectionString);

if (dbConnectionString != null)
{
    logger.LogInformation("Start DB migrating ...");

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<FileAppDbContext>();

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