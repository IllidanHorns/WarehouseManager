using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using WarehouseManager.AdminWeb.Models.Api;

namespace WarehouseManager.AdminWeb.Services.Api;

public abstract class ApiClientBase
{
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    protected ApiClientBase(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected HttpClient HttpClient { get; }

    protected async Task<T> GetAsync<T>(string uri, object? query = null, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(uri, query);
        var response = await HttpClient.GetAsync(url, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    protected async Task<TResponse> PostAsync<TRequest, TResponse>(
        string uri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        using var content = JsonContent.Create(payload, options: _serializerOptions);
        var response = await HttpClient.PostAsync(uri, content, cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    protected async Task<TResponse> PutAsync<TRequest, TResponse>(
        string uri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        using var content = JsonContent.Create(payload, options: _serializerOptions);
        var response = await HttpClient.PutAsync(uri, content, cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    protected async Task<TResponse> PatchAsync<TRequest, TResponse>(
        string uri,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        using var content = JsonContent.Create(payload, options: _serializerOptions);
        using var request = new HttpRequestMessage(HttpMethod.Patch, uri) { Content = content };
        var response = await HttpClient.SendAsync(request, cancellationToken);
        return await ReadAsync<TResponse>(response, cancellationToken);
    }

    protected async Task DeleteAsync(string uri, object? query = null, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(uri, query);
        var response = await HttpClient.DeleteAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw await CreateException(response, cancellationToken);
    }

    private static string BuildUrl(string uri, object? query)
    {
        if (query == null)
            return uri;

        var dictionary = ToDictionary(query);
        return QueryHelpers.AddQueryString(uri, dictionary!);
    }

    private async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var value = await response.Content.ReadFromJsonAsync<T>(_serializerOptions, cancellationToken);
            if (value is null)
                throw new ApiException(response.StatusCode, "Пустой ответ от сервера");

            return value;
        }

        throw await CreateException(response, cancellationToken);
    }

    private async Task<ApiException> CreateException(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        ApiErrorResponse? error = null;

        try
        {
            error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_serializerOptions, cancellationToken);
        }
        catch
        {
            // ignored
        }

        var message = error?.Message;

        if (string.IsNullOrWhiteSpace(message))
        {
            message = response.ReasonPhrase ?? $"Ошибка {(int)response.StatusCode}";
        }

        return new ApiException(response.StatusCode, message, error);
    }

    private static IDictionary<string, string?> ToDictionary(object query)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in query.GetType().GetProperties())
        {
            var value = property.GetValue(query);
            if (value is null)
                continue;

            if (value is string s)
            {
                if (!string.IsNullOrWhiteSpace(s))
                    result[property.Name] = s;
            }
            else if (value is IEnumerable<string> enumerable)
            {
                var joined = string.Join(",", enumerable);
                if (!string.IsNullOrWhiteSpace(joined))
                    result[property.Name] = joined;
            }
            else
            {
                result[property.Name] = Convert.ToString(value);
            }
        }

        return result;
    }
}

