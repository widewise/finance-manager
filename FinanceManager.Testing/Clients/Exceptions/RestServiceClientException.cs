using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace FinanceManager.Testing.Clients.Exceptions;

[Serializable]
[ExcludeFromCodeCoverage]
public class RestServiceClientException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public RestServiceClientException(
        string restMethod,
        Uri uri,
        string correlationId,
        Exception exception)
        : base($"{restMethod}: An exception has occurred while attempting to put request {uri} with identifier {correlationId}", exception)
    {
        StatusCode = HttpStatusCode.InternalServerError;
    }

    public RestServiceClientException(
        string restMethod,
        Uri uri,
        string correlationId,
        string reason,
        HttpResponseMessage responseMessage)
        : base($"{restMethod}: The status code of sending to {uri} HTTP response is {responseMessage.StatusCode}. Correlation identifier: {correlationId}. Reason: {reason}. Response: {responseMessage}")
    {
        StatusCode = responseMessage.StatusCode;
    }
}