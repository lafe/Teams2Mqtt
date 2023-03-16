using System.Text.Json.Serialization;
using lafe.Teams2Mqtt.Attributes;

namespace lafe.Teams2Mqtt.Mqtt.ComponentConfiguration;

public class HomeAssistantDevice
{
    /// <summary>
    /// A link to the webpage that can manage the configuration of this device.Can be either an HTTP or HTTPS link.
    /// </summary>
    [JsonPropertyName("configuration_url"), Optional]
    public string? ConfigurationUrl { get; set; }
    /// <summary>
    /// The hardware version of the device.
    /// </summary>
    [JsonPropertyName("hw_version"), Optional]
    public string? HardwareVersion { get; set; }
    /// <summary>
    /// A list of IDs that uniquely identify the device. For example a serial number.
    /// </summary>
    [JsonPropertyName("identifiers"), Optional]
    public string[]? Identifiers { get; set; }
    /// <summary>
    /// The manufacturer of the device.
    /// </summary>
    [JsonPropertyName("manufacturer"), Optional]
    public string? Manufacturer { get; set; }
    /// <summary>
    /// The model of the device.
    /// </summary>
    [JsonPropertyName("model"), Optional]
    public string? Model { get; set; }
    /// <summary>
    /// The name of the device.
    /// </summary>
    [JsonPropertyName("name"), Optional]
    public string? Name { get; set; }
    /// <summary>
    /// Suggest an area if the device isn’t in one yet.
    /// </summary>
    [JsonPropertyName("suggested_area"), Optional]
    public string? SuggestedArea { get; set; }
    /// <summary>
    /// The firmware version of the device.
    /// </summary>
    [JsonPropertyName("sw_version"), Optional]
    public string? SoftwareVersion { get; set; }
}