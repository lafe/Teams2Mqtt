using System.ComponentModel.Design;

namespace lafe.Teams2Mqtt.Attributes;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public class HomeAssistantSwitchAttribute : HomeAssistantComponentAttribute {

    /// <summary>
    /// The ID of the command that is part of the command topic for MQTT
    /// </summary>
    public string CommandId { get; set; }

    public HomeAssistantSwitchAttribute(string componentId, string commandId) 
        : base(componentId)
    {
        CommandId = commandId;
    }

    public HomeAssistantSwitchAttribute(string componentId, string commandId, string name)
        : base(componentId, name)
    {
        CommandId = commandId;
    }

    public HomeAssistantSwitchAttribute(string componentId, string commandId, string name, string localizationKey) 
        : base(componentId, name, localizationKey)
    {
        CommandId = commandId;
    }
}