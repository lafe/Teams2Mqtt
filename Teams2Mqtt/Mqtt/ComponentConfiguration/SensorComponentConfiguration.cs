using lafe.Teams2Mqtt.Attributes;
using System.Text.Json.Serialization;
using lafe.Teams2Mqtt.Converters;

namespace lafe.Teams2Mqtt.Mqtt.ComponentConfiguration;

public class SensorComponentConfiguration : BaseComponentConfiguration
{
    /// <summary>
    /// A list of MQTT topics subscribed to receive availability (online/offline) updates.
    /// </summary>
    [JsonPropertyName("availability"), Optional]
    public HomeAssistantAvailability[]? Availability { get; set; }

    /// <summary>
    /// Information about the device this sensor is a part of to tie it into the <a href="https://developers.home-assistant.io/docs/device_registry_index/">device registry</a>.
    /// </summary>
    [JsonPropertyName("device"), Optional]
    public HomeAssistantDevice? Device { get; set; }

    /// <summary>
    /// The type/class of the sensor to set the icon in the frontend.
    /// </summary>
    [JsonPropertyName("device_class"), Optional, JsonConverter(typeof(JsonStringEnumNameConverter<SensorDeviceClass>))]
    public SensorDeviceClass? DeviceClass { get; set; }

    /// <summary>
    /// Flag which defines if the entity should be enabled when first added.
    /// </summary>
    [JsonPropertyName("enabled_by_default"), Optional]
    public bool EnabledByDefault { get; set; } = true;

    /// <summary>
    /// Any icon from MaterialDesignIcons.com. Prefix name with mdi:, ie mdi:home.
    /// </summary>
    [JsonPropertyName("icon"), Optional]
    public string? Icon { get; set; }

    /// <summary>
    /// The options that are available if this is an enumeration
    /// </summary>
    [JsonPropertyName("options"), Optional]
    public string[]? Options { get; set; }

    /// <summary>
    /// The state_class of the sensor.
    /// </summary>
    [JsonPropertyName("state_class"), Optional, JsonConverter(typeof(JsonStringEnumNameConverter<StateClass>))]
    public StateClass? StateClass { get; set; }

    /// <summary>
    /// The MQTT topic subscribed to receive sensor values.
    /// </summary>
    [JsonPropertyName("state_topic"), Required]
    public string? StateTopic { get; set; }

    /// <summary>
    /// Defines the units of measurement of the sensor, if any.
    /// </summary>
    [JsonPropertyName("unit_of_measurement"), Optional]
    public string? UnitOfMeasurement { get; set; }

    /// <summary>
    /// Defines a template to extract the value.If the template throws an error, the current state will be used instead.
    /// </summary>
    [JsonPropertyName("value_template"), Optional]
    public string? ValueTemplate { get; set; }
}