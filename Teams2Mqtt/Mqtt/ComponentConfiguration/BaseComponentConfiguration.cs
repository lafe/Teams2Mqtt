using System.Text.Json.Serialization;
using lafe.Teams2Mqtt.Attributes;

namespace lafe.Teams2Mqtt.Mqtt.ComponentConfiguration;

public abstract class BaseComponentConfiguration
{
    /// <summary>
    /// The name of the MQTT sensor.
    /// </summary>
    [JsonPropertyName("name"), Optional]
    public string? Name { get; set; }

    /// <summary>
    /// An ID that uniquely identifies this sensor.If two sensors have the same unique ID, Home Assistant will raise an exception.
    /// </summary>
    [JsonPropertyName("unique_id"), Optional]
    public string? UniqueId { get; set; }
}