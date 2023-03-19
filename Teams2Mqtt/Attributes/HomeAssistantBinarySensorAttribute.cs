namespace lafe.Teams2Mqtt.Attributes;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public class HomeAssistantBinarySensorAttribute: HomeAssistantComponentAttribute {
    public HomeAssistantBinarySensorAttribute(string componentId) : base(componentId)
    {
    }

    public HomeAssistantBinarySensorAttribute(string componentId, string name) : base(componentId, name)
    {
    }

    public HomeAssistantBinarySensorAttribute(string componentId, string name, string localizationKey) : base(componentId, name, localizationKey)
    {
    }
}