using System.Reflection;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Extensions.ManagedClient;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using lafe.Teams2Mqtt.Attributes;
using lafe.Teams2Mqtt.Model;
using lafe.Teams2Mqtt.Mqtt;
using lafe.Teams2Mqtt.Mqtt.ComponentConfiguration;
using Microsoft.Extensions.Logging;

namespace lafe.Teams2Mqtt.Services;

public class MqttService : IDisposable
{
    protected ILogger<MqttService> Logger { get; }
    protected List<SensorLocalizationConfiguration> SensorLocalizations { get; }
    protected MqttLoggerFactory LoggerFactory { get; }
    protected MqttConfiguration? MqttConfiguration { get; }

    protected IManagedMqttClient? MqttClient { get; private set; }

    protected JsonSerializerOptions DefaultJsonSerializerOptions { get; }

    /// <summary>
    /// Determines if MQTT publishing is active
    /// </summary>
    protected bool Enabled { get; private set; } = true;

    public MqttService(ILogger<MqttService> logger, IOptions<MqttConfiguration> mqttConfiguration, IOptions<List<SensorLocalizationConfiguration>> sensorLocalizations, MqttLoggerFactory loggerFactory)
    {
        Logger = logger;
        SensorLocalizations = sensorLocalizations.Value;
        LoggerFactory = loggerFactory;
        MqttConfiguration = mqttConfiguration.Value;

        DefaultJsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
    
    public async Task StartAsync()
    {
        Logger.LogDebug(LogNumbers.MqttService.StartAsync, $"Initializing the MQTT configuration");

        var mqttFactoryLogger = LoggerFactory.CreateLogger();
        var mqttFactory = new MqttFactory(mqttFactoryLogger);

        var mqttLogger = LoggerFactory.CreateLogger();
        MqttClient = mqttFactory.CreateManagedMqttClient(mqttLogger);

        if (string.IsNullOrWhiteSpace(MqttConfiguration?.Server))
        {
            Logger.LogWarning(LogNumbers.MqttService.StartAsyncMqttServerNotConfigured, $"MQTT broker is not configured. Skipping initialization of MQTT client.");
            Enabled = false;
            return;
        }

        var availabilityTopic = AvailabilityTopic;

        Logger.LogInformation(LogNumbers.MqttService.StartAsyncConnectionConfiguration, $"Creating connection with MQTT broker {MqttConfiguration.Server}{(MqttConfiguration.Port != null ? $":{MqttConfiguration.Port}" : "")}");
        // Create options
        var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(MqttConfiguration.Server, MqttConfiguration.Port);

        // Add credentials (if given)
        if (!string.IsNullOrWhiteSpace(MqttConfiguration.UserName) && !string.IsNullOrWhiteSpace(MqttConfiguration.Password))
        {
            Logger.LogInformation(LogNumbers.MqttService.StartAsyncUsingCredentials, $"Using user \"{MqttConfiguration.UserName}\" for connection with MQTT broker {MqttConfiguration.Server}");
            mqttClientOptionsBuilder = mqttClientOptionsBuilder.WithCredentials(MqttConfiguration.UserName, MqttConfiguration.Password);
        }

        // Add Last Will and Testament
        mqttClientOptionsBuilder = mqttClientOptionsBuilder
            .WithWillTopic(availabilityTopic)
            .WithWillPayload("offline");

        var mqttClientOptions = mqttClientOptionsBuilder.Build();

        var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
            .WithClientOptions(mqttClientOptions)
            .Build();

        await MqttClient.StartAsync(managedMqttClientOptions);
    }
    
    /// <summary>
    /// Removes the automatic registration at Home Assistant and stops the MQTT Client
    /// </summary>
    public async Task StopAsync()
    {
        if (MqttClient == null)
        {
            Logger.LogInformation(LogNumbers.MqttService.StopAsyncNoClientAvailable, $"MQTT client not initialized. No client to stop.");
            return;
        }

        if (!MqttClient.IsStarted)
        {
            Logger.LogInformation(LogNumbers.MqttService.StopAsyncMqttClientNotStarted, $"MQTT client is not started. Skipping stopping of client.");
            return;
        }
        
        await SendOfflineAvailabilityMessageAsync();
        await MqttClient.StopAsync(true);
    }

    /// <summary>
    /// Publishes a Home Assistant Discovery Message for the Teams state
    /// </summary>
    public async Task PublishDiscoveryMessagesAsync<T>()
    {
        using var scope = Logger.BeginScope($"{nameof(MqttService)}:{nameof(PublishDiscoveryMessagesAsync)}");
        var stateObjectType = typeof(T);
        try
        {
            Logger.LogTrace(LogNumbers.MqttService.PublishDiscoveryMessageAsync, $"Publishing Home Assistant Discovery messages for \"{stateObjectType.Name}\"");
            if (MqttClient == null)
            {
                Logger.LogWarning(LogNumbers.MqttService.PublishDiscoveryMessageAsyncMqttClientNull, $"No MQTT client present. Cannot send discovery message");
                return;
            }


            // Get all sensors from the state object. Sensors are all properties that have a SensorAttribute.
            var sensors = stateObjectType.GetProperties().Where(p => p.GetCustomAttribute<SensorAttribute>() != null).ToList();

            var device = new HomeAssistantDevice()
            {
                Name = "Teams2Mqtt",
                Identifiers = new[] { "Teams2Mqtt" },
                Model = "Microsoft Teams API",
                Manufacturer = "",
                SoftwareVersion = "",
                SuggestedArea = MqttConfiguration?.DeviceSuggestedArea
            };

            foreach (var sensorPropertyInfo in sensors)
            {
                var sensorInformation = sensorPropertyInfo.GetCustomAttribute<SensorAttribute>();
                if (sensorInformation == null || string.IsNullOrWhiteSpace(sensorInformation.SensorId))
                {
                    Logger.LogWarning(LogNumbers.MqttService.PublishDiscoveryMessagesAsyncSensorInformationEmpty, $"The sensor \"{sensorPropertyInfo.Name}\" has incomplete sensor information and will be skipped.");
                    continue;
                }

                var componentType = HomeAssistantComponent.Sensor;
                if (sensorPropertyInfo.PropertyType == typeof(bool))
                {
                    componentType = HomeAssistantComponent.BinarySensor;
                }

                var discoveryMessageTopic = GetDiscoveryTopic<T>(sensorInformation.SensorId, componentType);
                var sensorStateTopic = GetStateTopic<T>();
                Logger.LogInformation(LogNumbers.MqttService.PublishDiscoveryMessagesAsyncTopicsGenerated, $"Topics for sensor \"{sensorPropertyInfo.Name}\":\nDiscovery Message: {discoveryMessageTopic}\nSensor State: {sensorStateTopic}");

                await PublishConfigurationAsync<T>(sensorPropertyInfo, sensorInformation, device, sensorStateTopic, discoveryMessageTopic);
            }
            
            await SendOnlineAvailabilityMessageAsync();

            Logger.LogInformation(LogNumbers.MqttService.PublishDiscoveryMessageAsyncSuccess, $"Published all auto discovery messages for \"{stateObjectType.Name}\".");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.MqttService.PublishDiscoveryMessageAsyncException, ex, $"An error occurred while publishing auto discovery messages for \"{stateObjectType.Name}\": {ex}");
            throw;
        }
    }

    protected async Task PublishConfigurationAsync<T>(PropertyInfo sensorPropertyInfo, SensorAttribute sensorInformation, HomeAssistantDevice device, string sensorStateTopic, string discoveryMessageTopic)
    {
        var name = sensorInformation.Name ?? sensorPropertyInfo.Name;
        if (!string.IsNullOrWhiteSpace(sensorInformation.LocalizationKey))
        {
            var localization = SensorLocalizations.FirstOrDefault(sl => string.Equals(sl.SensorId, sensorInformation.SensorId))?.SensorName;
            name = localization ?? name;
        }

        var validatedSensorId = EnsureValidString(sensorInformation.SensorId ?? string.Empty);
        var uniqueId = $"{validatedSensorId}";

        var sensorConfiguration = new SensorComponentConfiguration()
        {
            Availability = new[] { AvailabilityConfiguration },
            StateTopic = sensorStateTopic,
            StateClass = StateClass.Measurement,
            Device = device,
            Name = name.Trim(),
            UniqueId = uniqueId,
            Icon = sensorInformation.Icon,
            EnabledByDefault = sensorInformation.EnabledByDefault,
            ValueTemplate = $"{{{{ value_json.{uniqueId} }}}}",
        };

        var messagePayload = JsonSerializer.Serialize(sensorConfiguration, DefaultJsonSerializerOptions);
        Logger.LogTrace(LogNumbers.MqttService.PublishConfigurationAsyncPayload, $"Payload for configuration of sensor \"{sensorInformation.SensorId}\": {messagePayload}");

        var configurationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(discoveryMessageTopic)
            .WithPayload(messagePayload)
            .WithRetainFlag(true)
            .Build();

        await MqttClient!.EnqueueAsync(configurationMessage);

        Logger.LogInformation(LogNumbers.MqttService.PublishConfigurationAsyncConfigPublished, $"Published auto discovery message for sensor \"{sensorInformation.SensorId}\"");
    }

    public async Task SendOnlineAvailabilityMessageAsync()
    {
        await SendAvailabilityMessageAsync("online");
    }
    public async Task SendOfflineAvailabilityMessageAsync()
    {
        await SendAvailabilityMessageAsync("offline");
    }

    /// <summary>
    /// Sends an availability message to the MQTT broker
    /// </summary>
    /// <param name="payload">The payload that should be sent to the availability topic</param>
    protected async Task SendAvailabilityMessageAsync(string payload)
    {
        using var scope = Logger.BeginScope($"{nameof(MqttService)}:{nameof(SendAvailabilityMessageAsync)}");
        try
        {
            Logger.LogTrace(LogNumbers.MqttService.SendAvailabilityMessageAsync, $"Sending availability message to broker");
            if (MqttClient == null)
            {
                Logger.LogTrace(LogNumbers.MqttService.SendAvailabilityMessageAsyncMqttClientEmpty, $"MQTT client is empty. Skipping sending the availability message.");
                return;
            }

            var availabilityTopic = AvailabilityTopic;
            var availabilityMessage = new MqttApplicationMessageBuilder()
                .WithTopic(availabilityTopic)
                .WithPayload(payload)
                .Build();

            await MqttClient.EnqueueAsync(availabilityMessage);

            Logger.LogInformation(LogNumbers.MqttService.SendAvailabilityMessageAsyncSuccess, $"Sent availability message \"{payload}\" to broker on topic \"{availabilityTopic}\"");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.MqttService.SendAvailabilityMessageAsyncException, ex, $"An error occurred while sending the availability message to the broker: {ex}");
        }
    }

    /// <summary>
    /// Sends updated values to the MQTT broker for the given <paramref name="updatedEntity"/>
    /// </summary>
    /// <param name="updatedEntity">The <see cref="updatedEntity"/> for which the update message should be sent</param>
    public async Task SendUpdatesAsync<T>(T updatedEntity)
    {
        var updatedEntityType = typeof(T);
        using var scope = Logger.BeginScope($"{nameof(MqttService)}:{nameof(SendUpdatesAsync)}");
        try
        {
            if (!Enabled)
            {
                return;
            }

            Logger.LogTrace(LogNumbers.MqttService.SendUpdatesAsync, $"Sending updated values for \"{updatedEntityType.Name}\"");
            if (MqttClient == null || !MqttClient.IsStarted || updatedEntity == null)
            {
                return;
            }

            await SendOnlineAvailabilityMessageAsync();

            var sensorStateTopic = GetStateTopic<T>();
            Logger.LogTrace(LogNumbers.MqttService.SendUpdatesAsyncSensorStateTopic, $"Using topic \"{sensorStateTopic}\"");

            // Get all sensors from the state object. Sensors are all properties that have a SensorAttribute.
            var sensors = updatedEntityType.GetProperties().Where(p => p.GetCustomAttribute<SensorAttribute>() != null).ToList();
            var sensorUpdateMessage = new Dictionary<string, object>();
            foreach (var sensorPropertyInfo in sensors)
            {
                var sensorInformation = sensorPropertyInfo.GetCustomAttribute<SensorAttribute>();
                if (sensorInformation == null || string.IsNullOrWhiteSpace(sensorInformation.SensorId))
                {
                    Logger.LogWarning(LogNumbers.MqttService.SendUpdatesAsyncSensorInformationEmpty, $"The sensor \"{sensorPropertyInfo.Name}\" has incomplete sensor information and will be skipped.");
                    continue;
                }

                var entityValue = (bool)(sensorPropertyInfo.GetValue(updatedEntity) ?? false);

                var value = entityValue ? "ON" : "OFF";

                sensorUpdateMessage.Add(sensorInformation.SensorId, value);
            }

            var statePayload = JsonSerializer.Serialize(sensorUpdateMessage, DefaultJsonSerializerOptions);
            Logger.LogTrace(LogNumbers.MqttService.SendUpdatesAsyncPayload, $"Payload for update of \"{updatedEntityType.Name}\": {statePayload}");
            
            var updateMessage = new MqttApplicationMessageBuilder()
                .WithTopic(sensorStateTopic)
                .WithPayload(statePayload)
                .Build();

            await MqttClient.EnqueueAsync(updateMessage);

            Logger.LogTrace(LogNumbers.MqttService.SendUpdatesAsyncUpdatePublished, $"Published state update for \"{updatedEntityType.Name}\"");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.MqttService.SendUpdatesAsyncException, ex, $"An error occurred while updating the state for \"{updatedEntityType.Name}\": {ex}");
        }
    }

    /// <summary>
    /// Removes a Home Assistant Discovery Message for the given type by publishing an empty message on the broker
    /// </summary>
    public async Task RemoveDiscoveryMessageAsync<T>()
    {
        using var scope = Logger.BeginScope($"{nameof(MqttService)}:{nameof(RemoveDiscoveryMessageAsync)}");
        var stateObjectType = typeof(T);
        try
        {
            if (!(MqttConfiguration?.RemoveDevicesOnShutdown ?? false))
            {
                Logger.LogInformation(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncRemoveDeviceOnShutdownDisabled, $"The \"Remove Device On Shutdown\" flag is not set. Existing device registrations will not be removed from Home Assistant.");
                return;
            }

            if (MqttClient == null)
            {
                Logger.LogWarning(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncMqttClientNull, $"No MQTT client present. Cannot remove discovery message");
                return;
            }

            // Get all sensors from the state object. Sensors are all properties that have a SensorAttribute.
            var sensors = stateObjectType.GetProperties().Where(p => p.GetCustomAttribute<SensorAttribute>() != null).ToList();
            
            foreach (var sensorPropertyInfo in sensors)
            {
                var sensorInformation = sensorPropertyInfo.GetCustomAttribute<SensorAttribute>();
                if (sensorInformation == null || string.IsNullOrWhiteSpace(sensorInformation.SensorId))
                {
                    Logger.LogWarning(LogNumbers.MqttService.PublishDiscoveryMessagesAsyncSensorInformationEmpty, $"The sensor \"{sensorPropertyInfo.Name}\" has incomplete sensor information and will be skipped.");
                    continue;
                }

                var componentType = HomeAssistantComponent.Sensor;
                if (sensorPropertyInfo.PropertyType == typeof(bool))
                {
                    componentType = HomeAssistantComponent.BinarySensor;
                }

                var discoveryMessageTopic = GetDiscoveryTopic<T>(sensorInformation.SensorId, componentType);
                Logger.LogInformation(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncDiscoveryMessage, $"Discovery topic for sensor \"{sensorPropertyInfo.Name}\": {discoveryMessageTopic}");

                var sensorConfigurationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(discoveryMessageTopic)
                    .WithPayload("")
                    .WithRetainFlag(true)
                    .Build();

                await MqttClient.EnqueueAsync(sensorConfigurationMessage);
                Logger.LogInformation(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncRemovedSensorConfig, $"Removed sensor configuration auto discovery message for sensor \"{sensorPropertyInfo.Name}\"");

            }

            Logger.LogTrace(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncSuccess, $"Removed all auto discovery messages for \"{stateObjectType.Name}\"");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncException, ex, $"An error occurred while removing auto discovery messages for \"{stateObjectType.Name}\": {ex}");
            throw;
        }
    }

    public void Dispose()
    {
        MqttClient?.Dispose();
    }

    /// <summary>
    /// Generates the discovery topic for the given sensor, <paramref name="component"/> and type
    /// </summary>
    /// <param name="sensorId">The id of the sensor for which the discovery topic should be generated</param>
    /// <param name="component">The <see cref="HomeAssistantComponent"/> of the device</param>
    protected string GetDiscoveryTopic<T>(string sensorId, HomeAssistantComponent component)
    {
        var typeName = EnsureValidString(typeof(T).Name);
        var validatedSensorId = EnsureValidString(sensorId ?? string.Empty);
        var computerName = EnsureValidString(Environment.MachineName);
        var discoveryMessageTopic = new HomeAssistantDiscoveryTopicBuilder()
            .WithDiscoveryPrefix(MqttConfiguration?.HomeAssistantAutoDiscoveryTopic)
            .WithComponent(component)
            .WithObjectId($"Teams2Mqtt-{computerName}-{typeName}-{validatedSensorId}")
            .Build();
        return discoveryMessageTopic;
    }
    /// <summary>
    /// Generates the state topic for the given sensor
    /// </summary>
    protected string GetStateTopic<T>()
    {
        var typeName = EnsureValidString(typeof(T).Name);
        var computerName = EnsureValidString(Environment.MachineName);
        return $"teams2mqtt/sensor/{computerName}-{typeName}/state";
    }
    /// <summary>
    /// The topic that is used to broadcast the availability messages
    /// </summary>
    protected string AvailabilityTopic
    {
        get
        {
            var computerName = EnsureValidString(Environment.MachineName);
            return $"teams2mqtt/{computerName}/availability";
        }
    }

    /// <summary>
    /// The general Home Assistant availability configuration
    /// </summary>
    protected HomeAssistantAvailability AvailabilityConfiguration
    {
        get
        {
            var availabilityTopic = AvailabilityTopic;
            var availabilityConfiguration = new HomeAssistantAvailability()
            {
                PayloadAvailableText = "online",
                PayloadNotAvailableText = "offline",
                Topic = availabilityTopic
            };
            return availabilityConfiguration;
        }
    }

    /// <summary>
    /// Makes sure that the <paramref name="value"/> only contains alphanumeric characters (A-Z, a-z and 0-9) as well as underscore and hyphen
    /// </summary>
    /// <param name="value">The value to be checked</param>
    protected string EnsureValidString(string value)
    {
        var sb = new StringBuilder();
        foreach (char c in value)
        {
            if (Char.IsLetterOrDigit(c) || c == '_' || c == '-')
            {
                sb.Append(c);
            }
            else
            {
                sb.Append("_");
            }
        }
        return sb.ToString();
    }
}