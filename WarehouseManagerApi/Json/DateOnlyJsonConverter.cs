using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarehouseManagerApi.Json
{
    public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string DefaultFormat = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && reader.GetString() is { } value)
            {
                if (DateOnly.TryParse(value, out var result))
                {
                    return result;
                }

                if (DateTime.TryParse(value, out var dateTime))
                {
                    return DateOnly.FromDateTime(dateTime);
                }
            }

            throw new JsonException($"Unable to parse DateOnly value from '{reader.GetString()}'");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DefaultFormat));
        }
    }
}
