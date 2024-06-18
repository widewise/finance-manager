using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;
using FinanceManager.Events;
using FinanceManager.Events.Models;
using FinanceManager.Expense;
using FinanceManager.Expense.Consumers;
using FinanceManager.Expense.Services;
using FinanceManager.TransportLibrary;
using FinanceManager.TransportLibrary.Extensions;
using FinanceManager.Web.Extensions;
using FinanceManager.Web.Swagger;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddTransient<IExpenseService, ExpenseService>();

builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1.0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddMvc(
        options =>
        {
            // automatically applies an api version based on the name of
            // the defining controller's namespace
            options.Conventions.Add(new VersionByNamespaceConvention());
        })
    .AddApiExplorer();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<NamedSwaggerGenOptions<Program>>();

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
    FinanceIdentityConstants.ExpenseTitle,
    FinanceIdentityConstants.ExpenseAudience);
builder.Services.AddCommon();
builder.Services.AddTransportCore(builder.Configuration);
builder.Services.AddTransportPublisher<ChangeAccountBalanceEvent>(EventConstants.AccountExchange);
builder.Services.AddTransportPublisher<AddExpenseRejectEvent>(EventConstants.FileExchange);

builder.Services.AddTransportConsumerWithReject<AddExpenseEvent, AddExpenseRejectEvent, AddExpenseConsumer>(
    EventConstants.FileExchange,
    EventConstants.ExpenseExchange,
    EventConstants.AddExpenseEvent);

builder.Services.AddTransportConsumer<DeleteExpenseEvent, DeleteExpenseConsumer>(
    EventConstants.ExpenseExchange,
    EventConstants.DeleteExpenseEvent);

builder.Services.AddTransportConsumer<ChangeAccountBalanceRejectEvent, ChangeAccountBalanceRejectConsumer>(
    EventConstants.DepositExpenseCommonExchange,
    EventConstants.ExpenseChangeAccountBalanceRejectedEvent);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("DB connection string: {DBConnectionString}", dbConnectionString);

if (dbConnectionString != null)
{
    logger.LogInformation("Start DB migrating ...");

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    // Here is the migration executed
    await dbContext.Database.MigrateAsync();

    logger.LogInformation("DB migrated");
}

app.Services.BuildTransportMap();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var groupName in provider.ApiVersionDescriptions.Select(x => x.GroupName))
        {
            options.SwaggerEndpoint(
                $"/swagger/{groupName}/swagger.json",
                $"v{groupName.ToUpperInvariant()}");
        }
    });
}

app.UseExceptionHandler(a => a.AddDefaultExceptionHandler(logger));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();