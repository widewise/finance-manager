using System.Diagnostics;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FinanceManager.Web.Swagger;

public class NamedSwaggerGenOptions<TEntryPoint> : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly string? _versionString;
    private readonly string? _apiName;

    public NamedSwaggerGenOptions(IApiVersionDescriptionProvider provider)
    {
        var info = FileVersionInfo.GetVersionInfo(typeof(TEntryPoint).Assembly.Location);
        _versionString = info.FileVersion;
        _apiName = info.ProductName;
        _provider = provider;
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    public void Configure(SwaggerGenOptions options)
    {
        // add swagger document for every API version discovered
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                CreateVersionInfo(description));
        }
    }

    private OpenApiInfo CreateVersionInfo(
        ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = $"{_apiName} API v{description.GroupName}",
            Version = _versionString
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }
}