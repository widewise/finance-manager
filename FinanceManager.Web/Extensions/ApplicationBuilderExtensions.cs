using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceManager.Web.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder AddDefaultExceptionHandler(
        this IApplicationBuilder builder,
        ILogger logger)
    {
        builder.Run(context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error == null)
            {
                return Task.CompletedTask;
            }

            logger.LogError(exceptionHandlerPathFeature.Error, 
                "Call endpoint {EndpointPath} error: {ErrorMessage}",
                exceptionHandlerPathFeature.Path,
                exceptionHandlerPathFeature.Error.Message);

            return Task.CompletedTask;
        });

        return builder;
    }
}