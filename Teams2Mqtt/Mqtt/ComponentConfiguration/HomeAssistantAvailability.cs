using lafe.Teams2Mqtt.Attributes;
using System.Text.Json.Serialization;

namespace lafe.Teams2Mqtt.Mqtt.ComponentConfiguration;

public class HomeAssistantAvailability
{
    /// <summary>
    /// The payload that represents the available state. The default value is "online"
    /// </summary>
    [JsonPropertyName("payload_available"), Optional]
    public string PayloadAvailableText { get; set; } = "online";

    /// <summary>
    /// The payload that represents the unavailable state. The default value is "offline"
    /// </summary>
    [JsonPropertyName("payload_not_available"), Optional]
    public string PayloadNotAvailableText { get; set; } = "offline";

    /// <summary>
    /// An MQTT topic subscribed to receive availability (online/offline) updates.
    /// </summary>
    [JsonPropertyName("topic"), Required]
    public string? Topic { get; set; }

    /// <summary>
    /// Defines a template to extract device’s availability from the topic. To determine the devices’s availability result of this template will be compared to payload_available and payload_not_available.
    /// </summary>
    [JsonPropertyName("value_template"), Optional]
    public string? ValueTemplate { get; set; }
}