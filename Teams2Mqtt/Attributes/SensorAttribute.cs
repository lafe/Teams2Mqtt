using lafe.Teams2Mqtt.Mqtt;

namespace lafe.Teams2Mqtt.Attributes;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public class SensorAttribute : Attribute
{
    /// <summary>
    /// The unique ID of the sensor
    /// </summary>
    public string SensorId { get; set; }

    /// <summary>
    /// The default name of the sensor in Home Assistant
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The key of the Sensor Localizations array
    /// </summary>
    public string? LocalizationKey { get; set; }

    /// <summary>
    /// Determines if the sensor should be enabled by default. The default value is <c>true</c>.
    /// </summary>
    public bool EnabledByDefault { get; set; } = true;
    
    /// <summary>
    /// Specifies the icon of the sensor
    /// </summary>
    public string? Icon { get; set; }

    public SensorAttribute(string sensorId)
    {
        SensorId = sensorId;
    }

    public SensorAttribute(string sensorId, string name)
    : this(sensorId)
    {
        Name = name;
    }

    public SensorAttribute(string sensorId, string name, string localizationKey)
        : this(sensorId, name)
    {
        LocalizationKey = localizationKey;
    }
}