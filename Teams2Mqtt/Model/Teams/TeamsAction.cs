using System.Text.Json.Serialization;

namespace lafe.Teams2Mqtt.Model.Teams;

public class TeamsAction
{
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = "1.0.0";
    [JsonPropertyName("service")]
    public string Service { get; set; }
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("manufacturer")]
    public string Manufacturer => "lafe";
    [JsonPropertyName("device")]
    public string Device => Environment.MachineName ?? string.Empty;
    [JsonPropertyName("timestamp")]
    public long Timestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public TeamsAction(string service, string action)
    {
        Service = service;
        Action = action;
    }
}