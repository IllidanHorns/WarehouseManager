using System.Text.Json.Serialization;

namespace WarehouseManager.AdminWeb.Models.Api;

public class ApiErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errors")]
    public List<ApiErrorDetail>? Errors { get; set; }

    public class ApiErrorDetail
    {
        [JsonPropertyName("propertyName")]
        public string? PropertyName { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }
    }
}

