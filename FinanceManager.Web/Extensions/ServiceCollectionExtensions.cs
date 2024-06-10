using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using FinanceManager.Web.Models;
using FinanceManager.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace FinanceManager.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        string serviceAudience,
        string serviceVersion = "v1")
    {
        var settings = new CustomAuthenticationSettings();
        var section = configuration.GetSection(CustomAuthenticationSettings.SectionName);
        if (section == null)
        {
            throw new Exception($"Can't find section {CustomAuthenticationSettings.SectionName}");
        }

        section.Bind(settings);

        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromSeconds(10);
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = settings.IdentityUrl;
                options.Audience = serviceAudience;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Constants.ValidIssuer,
                    ValidAudience = Constants.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key))
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var securityStamp = context.Principal.FindFirstValue("AspNet.Identity.SecurityStamp");
                        if (string.IsNullOrEmpty(securityStamp))
                        {
                            context.Fail("Token security stamp mismatch");
                        }
                    }
                };

            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.RequireAdmin, policy => policy.RequireRole(UserRole.Admin.DisplayName()));
            options.AddPolicy(AuthPolicies.RequireUser, policy => policy.RequireRole(UserRole.User.DisplayName()));
            options.AddPolicy(AuthPolicies.RequireCategoryRead, policy => policy.RequireClaim(AuthClaimTypes.Permission, AuthClaims.CategoryRead));
            options.AddPolicy(AuthPolicies.RequireCategoryWrite, policy => policy.RequireClaim(AuthClaimTypes.Permission, AuthClaims.CategoryWrite));
            options.AddPolicy(AuthPolicies.RequireCurrencyRead, policy => policy.RequireClaim(AuthClaimTypes.Permission, AuthClaims.CurrencyRead));
            options.AddPolicy(AuthPolicies.RequireCurrencyWrite, policy => policy.RequireClaim(AuthClaimTypes.Permission, AuthClaims.CurrencyWrite));
        });

        services.AddAuthorization();
        services.AddSwaggerWithAuthentication(serviceName, serviceVersion);

        return services;
    }

    public static IServiceCollection AddSwaggerWithAuthentication(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion = "v1")
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(serviceVersion, new OpenApiInfo
            {
                Title = serviceName,
                Version = serviceVersion
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter Bearer Token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddApiKeyAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = new ApiKeySettings();
        configuration.GetSection(ApiKeySettings.Section).Bind(settings);
        services.AddSingleton(settings);
        services.AddSingleton<ApiKeyAuthorizationFilter>();
        services.AddSingleton<IApiKeyValidator, ApiKeyValidator>();

        return services;
    }
}