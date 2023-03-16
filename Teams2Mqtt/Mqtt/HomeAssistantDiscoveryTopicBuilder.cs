using lafe.Teams2Mqtt.Attributes;
using lafe.Teams2Mqtt.Extensions;

namespace lafe.Teams2Mqtt.Mqtt;

public class HomeAssistantDiscoveryTopicBuilder
{
    public HomeAssistantDiscoveryTopicBuilder()
    {
    }

    public string DiscoveryPrefix { get; private set; } = "homeassistant";
    public HomeAssistantComponent? Component { get; private set; }
    public string? NodeId { get; private set; }
    public string? ObjectId { get; private set; }

    /// <summary>
    /// The prefix for discovery topics in Home Assistant. Default value is "homeassistant".
    /// </summary>
    public HomeAssistantDiscoveryTopicBuilder WithDiscoveryPrefix(string? prefix)
    {
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            DiscoveryPrefix = prefix;
        }

        return this;
    }

    /// <summary>
    /// Sets the component to one of the supported MQTT components
    /// </summary>
    public HomeAssistantDiscoveryTopicBuilder WithComponent(HomeAssistantComponent? component)
    {
        if (component != null)
        {
            Component = component;
        }

        return this;
    }

    /// <summary>
    /// Sets the ID of the node providing the topic. This is not used by Home Assistant but may be used to structure the MQTT topic.
    /// </summary>
    public HomeAssistantDiscoveryTopicBuilder WithNode(string? nodeId)
    {
        if (nodeId != null)
        {
            NodeId = nodeId;
        }

        return this;
    }

    /// <summary>
    /// Sets the ID of the device. This is only to allow for separate topics for each device and is not used for the entity_id. The ID of the device must only consist of characters from the character class [a-zA-Z0-9_-] (alphanumerics, underscore and hyphen).
    /// </summary>
    public HomeAssistantDiscoveryTopicBuilder WithObjectId(string? objectId)
    {
        if (objectId != null)
        {
            ObjectId = objectId;
        }

        return this;
    }

    public string Build()
    {
        if (Component == null)
        {
            throw new NullReferenceException("The Component is missing");
        }

        if (ObjectId == null)
        {
            throw new NullReferenceException("The ObjectId is missing");
        }

        var nameAttribute = Component.GetAttribute<HomeAssistantComponent, NameAttribute>();
        var componentName = nameAttribute?.Name;

        if (string.IsNullOrWhiteSpace(NodeId))
        {
            return $"{DiscoveryPrefix}/{componentName}/{ObjectId}/config";
        }
        else
        {
            return $"{DiscoveryPrefix}/{componentName}/{NodeId}/{ObjectId}/config";
        }
    }
}