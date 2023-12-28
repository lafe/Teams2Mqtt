namespace lafe.Teams2Mqtt.Model;

public class MqttConfiguration
{
    /// <summary>
    /// If enabled, the MQTTnet client will also log messages. The output depends on the additional logging configuration performed for the .NET logging library. Default value is false.
    /// </summary>
    public bool MqttLoggingEnabled { get; set; } = false;
    /// <summary>
    /// URL to the Websocket MQTT Host, disables the <see cref="Server"/> and the <see cref="Port"/> Property
    /// </summary>
    public Uri? WebsocketUri { get; set; }
    /// <summary>
    /// The MQTT broker to connect to
    /// </summary>
    public string? Server { get; set; }
    /// <summary>
    /// The port to connect to
    /// </summary>
    public int? Port { get; set; }
    /// <summary>
    /// The username to use for authentication with the MQTT broker
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// The password to use for authentication with the MQTT broker
    /// </summary>
    public string? Password { get; set; }
    /// <summary>
    /// The topic to use for MQTT auto discovery by Home Assistant. The default value is "homeassistant".
    /// See https://www.home-assistant.io/integrations/mqtt/#mqtt-discovery for further information.
    /// </summary>
    public string HomeAssistantAutoDiscoveryTopic { get; set; } = "homeassistant";
    /// <summary>
    /// If enabled, the devices will be removed from Home Assistant when the service is stopped. Default value is false.
    /// </summary>
    public bool RemoveDevicesOnShutdown { get; set; } = false;
    /// <summary>
    /// The suggested area for the created device in Home Assistant.
    /// </summary>
    public string? DeviceSuggestedArea { get; set; }
}