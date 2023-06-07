using System.Text.Json.Serialization;

namespace RPA.MIT.Notification.Function.Services;

public class Event
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;
    [JsonPropertyName("properties")]
    public EventProperties Properties { get; init; } = default!;
}