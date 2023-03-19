namespace lafe.Teams2Mqtt.Attributes;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public class HomeAssistantSwitchAttribute : HomeAssistantComponentAttribute {
    public HomeAssistantSwitchAttribute(string componentId) : base(componentId)
    {
    }

    public HomeAssistantSwitchAttribute(string componentId, string name) : base(componentId, name)
    {
    }

    public HomeAssistantSwitchAttribute(string componentId, string name, string localizationKey) : base(componentId, name, localizationKey)
    {
    }
}