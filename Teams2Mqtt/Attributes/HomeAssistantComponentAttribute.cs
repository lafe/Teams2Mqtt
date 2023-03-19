using lafe.Teams2Mqtt.Mqtt;

namespace lafe.Teams2Mqtt.Attributes;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public class HomeAssistantComponentAttribute : Attribute
{
    /// <summary>
    /// The unique ID of the component
    /// </summary>
    public string ComponentId { get; set; }

    /// <summary>
    /// The default name of the component in Home Assistant
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The key of the component Localizations array
    /// </summary>
    public string? LocalizationKey { get; set; }

    /// <summary>
    /// Determines if the component should be enabled by default. The default value is <c>true</c>.
    /// </summary>
    public bool EnabledByDefault { get; set; } = true;

    /// <summary>
    /// Specifies the icon of the component
    /// </summary>
    public string? Icon { get; set; }

    public HomeAssistantComponentAttribute(string componentId)
    {
        ComponentId = componentId;
    }

    public HomeAssistantComponentAttribute(string componentId, string name)
    : this(componentId)
    {
        Name = name;
    }

    public HomeAssistantComponentAttribute(string componentId, string name, string localizationKey)
        : this(componentId, name)
    {
        LocalizationKey = localizationKey;
    }
}