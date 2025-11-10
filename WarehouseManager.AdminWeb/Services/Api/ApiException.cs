using System.Net;
using WarehouseManager.AdminWeb.Models.Api;

namespace WarehouseManager.AdminWeb.Services.Api;

public class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string message, ApiErrorResponse? errorResponse = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorResponse = errorResponse;
    }

    public HttpStatusCode StatusCode { get; }

    public ApiErrorResponse? ErrorResponse { get; }

    public IEnumerable<KeyValuePair<string, string>> GetValidationErrors()
    {
        if (ErrorResponse?.Errors == null)
            yield break;

        foreach (var detail in ErrorResponse.Errors)
        {
            var key = detail.PropertyName ?? string.Empty;
            var value = detail.ErrorMessage ?? string.Empty;
            yield return new KeyValuePair<string, string>(key, value);
        }
    }
}

