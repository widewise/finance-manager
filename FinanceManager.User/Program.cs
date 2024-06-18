using System.Reflection;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;
using FinanceManager.User;
using FinanceManager.User.Migrations;
using FinanceManager.User.Models;
using FinanceManager.User.Services;
using FinanceManager.Web;
using FinanceManager.Web.Extensions;
using FinanceManager.Web.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (dbConnectionString == null)
{
    throw new Exception("Connection string to DB is null");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dbConnectionString));

builder.Services.AddTransient<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddIdentity<User, CustomIdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

builder.Services
    .AddIdentityServer(options =>
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;
        options.EmitStaticAudienceClaim = true;
    })
    .AddAspNetIdentity<User>()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseNpgsql(dbConnectionString, 
                sql => sql.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseNpgsql(dbConnectionString, 
                sql => sql.MigrationsAssembly(migrationsAssembly));

        options.EnableTokenCleanup = true;
        options.TokenCleanupInterval = 300;
    })
    .AddInMemoryIdentityResources(Resources.GetIdentityResources())
    .AddInMemoryApiResources(Resources.GetApiResources())
    .AddInMemoryApiScopes(Resources.GetApiScopes())
    .AddDeveloperSigningCredential();
var apiSecurityKey = builder.Configuration.GetValue<string>("AppSecurityKey");
if (string.IsNullOrEmpty(apiSecurityKey))
{
    throw new Exception("API security key is null or empty");
}
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromSeconds(10);
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = Constants.ValidIssuer,
            ValidAudience = Constants.ValidAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiSecurityKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var user = await userManager.GetUserAsync(context.Principal!);
                if (user != null)
                {
                    var securityStamp = context.Principal.FindFirstValue("AspNet.Identity.SecurityStamp");
                    if (securityStamp != user.SecurityStamp)
                    {
                        context.Fail("Token security stamp mismatch");
                    }
                }
            }
        };
    });

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithAuthentication("User API");
builder.Services.ConfigureOptions<NamedSwaggerGenOptions<Program>>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("DB connection string: {DBConnectionString}", dbConnectionString);

logger.LogInformation("Start DB migrating ...");

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider
    .GetRequiredService<AppDbContext>();

await dbContext.Database.MigrateAsync();
var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
await dbInitializer.InitializeAsync();

logger.LogInformation("DB migrated");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"v{description.GroupName.ToUpperInvariant()}");
        }
    });
}

app.UseExceptionHandler(a => a.AddDefaultExceptionHandler(logger));

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseIdentityServer();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();