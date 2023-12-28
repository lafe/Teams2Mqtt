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
using MQTTnet.Protocol;
using lafe.Teams2Mqtt.Model.Teams;

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
    /// Saves a match between registered command topics and the associated Home Assistant Component configuration
    /// </summary>
    protected Dictionary<string, string> CommandTopicToCommandId { get; } = new Dictionary<string, string>();

    /// <summary>
    /// Determines if MQTT publishing is active
    /// </summary>
    protected bool Enabled { get; private set; } = true;


    public delegate void ActionReceivedHandler(object sender, string actionId, string payload);
    /// <summary>
    /// Is executed when an action has been received through MQTT
    /// </summary>
    public event ActionReceivedHandler? ActionReceived;

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

        var availabilityTopic = AvailabilityTopic;

        Logger.LogInformation(LogNumbers.MqttService.StartAsyncConnectionConfiguration, $"Creating connection with MQTT broker {MqttConfiguration.Server}{(MqttConfiguration.Port != null ? $":{MqttConfiguration.Port}" : "")}");
        // Create options
        var mqttClientOptionsBuilder = new MqttClientOptionsBuilder();

        // Websocket
        if (MqttConfiguration.WebsocketUri != null)
        {
            Logger.LogInformation(LogNumbers.MqttService.UseWebsocketMessage, "Using Mqtt over Websocket {WebsocketUri}", MqttConfiguration.WebsocketUri);
            mqttClientOptionsBuilder.WithWebSocketServer(MqttConfiguration.WebsocketUri.ToString());
        }
        // TCP
        else if (!string.IsNullOrWhiteSpace(MqttConfiguration?.Server))
        {
            mqttClientOptionsBuilder.WithTcpServer(MqttConfiguration.Server, MqttConfiguration.Port);
        }
        // Fallback
        else
        {
            Logger.LogWarning(LogNumbers.MqttService.StartAsyncMqttServerNotConfigured, $"MQTT broker is not configured. Skipping initialization of MQTT client.");
            Enabled = false;
            return;
        }

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
    public async Task PublishDiscoveryMessagesAsync<T>(CancellationToken cancellationToken = default)
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


            // Get all sensors from the state object. Sensors are all properties that have a HomeAssistantComponentAttribute.
            var sensors = stateObjectType.GetProperties().Where(p => p.GetCustomAttribute<HomeAssistantComponentAttribute>() != null).ToList();

            var device = new HomeAssistantDevice()
            {
                Name = "Teams2Mqtt",
                Identifiers = new[] { "Teams2Mqtt" },
                Model = "Microsoft Teams API",
                Manufacturer = "",
                SoftwareVersion = "",
                SuggestedArea = MqttConfiguration?.DeviceSuggestedArea
            };

            foreach (var componentPropertyInfo in sensors)
            {
                var componentInformation = componentPropertyInfo.GetCustomAttribute<HomeAssistantComponentAttribute>();
                if (componentInformation == null || string.IsNullOrWhiteSpace(componentInformation.ComponentId))
                {
                    Logger.LogWarning(LogNumbers.MqttService.PublishDiscoveryMessagesAsyncSensorInformationEmpty, $"The component \"{componentPropertyInfo.Name}\" has incomplete component information and will be skipped.");
                    continue;
                }

                var componentType = GetComponentType(componentInformation);

                var discoveryMessageTopic = GetDiscoveryTopic<T>(componentInformation.ComponentId, componentType);
                var componentStateTopic = GetStateTopic<T>();
                var commandTopic = "";
                if (componentType == HomeAssistantComponent.Switch && componentInformation is HomeAssistantSwitchAttribute switchComponentInformation)
                {
                    commandTopic = GetCommandTopic<T>(switchComponentInformation.CommandId);
                    CommandTopicToCommandId.Add(commandTopic, switchComponentInformation.CommandId);
                }

                Logger.LogInformation(LogNumbers.MqttService.PublishDiscoveryMessagesAsyncTopicsGenerated, $"Topics for component \"{componentPropertyInfo.Name}\":\nDiscovery Message: {discoveryMessageTopic}\nComponent State: {componentStateTopic}");

                await PublishConfigurationAsync<T>(componentPropertyInfo, componentInformation, componentType, device, componentStateTopic, discoveryMessageTopic, commandTopic);
            }

            await SendOnlineAvailabilityMessageAsync();
            Logger.LogInformation(LogNumbers.MqttService.PublishDiscoveryMessageAsyncSuccess, $"Published all auto discovery messages for \"{stateObjectType.Name}\".");

            MqttClient.ApplicationMessageReceivedAsync += HandleApplicationMessageReceived;
            await SubscribeToCommandTopicsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.MqttService.PublishDiscoveryMessageAsyncException, ex, $"An error occurred while publishing auto discovery messages for \"{stateObjectType.Name}\": {ex}");
            throw;
        }
    }

    protected HomeAssistantComponent GetComponentType(HomeAssistantComponentAttribute componentInfo)
    {
        switch (componentInfo.GetType().Name)
        {
            case $"{nameof(HomeAssistantBinarySensorAttribute)}":
                return HomeAssistantComponent.BinarySensor;
            case $"{nameof(HomeAssistantSwitchAttribute)}":
                return HomeAssistantComponent.Switch;
            default:
                return HomeAssistantComponent.Sensor;
        }
    }

    protected async Task PublishConfigurationAsync<T>(PropertyInfo componentPropertyInfo, HomeAssistantComponentAttribute componentInformation, HomeAssistantComponent componentType, HomeAssistantDevice device, string componentStateTopic, string discoveryMessageTopic, string commandTopic)
    {
        var name = componentInformation.Name ?? componentPropertyInfo.Name;
        if (!string.IsNullOrWhiteSpace(componentInformation.LocalizationKey))
        {
            var localization = SensorLocalizations.FirstOrDefault(sl => string.Equals(sl.SensorId, componentInformation.ComponentId))?.SensorName;
            name = localization ?? name;
        }

        var validatedComponentId = EnsureValidString(componentInformation.ComponentId ?? string.Empty);
        var uniqueId = $"{validatedComponentId}";

        var componentConfiguration = new SensorComponentConfiguration()
        {
            Availability = new[] { AvailabilityConfiguration },
            StateTopic = componentStateTopic,
            StateClass = StateClass.Measurement,
            Device = device,
            Name = name.Trim(),
            UniqueId = uniqueId,
            Icon = componentInformation.Icon,
            EnabledByDefault = componentInformation.EnabledByDefault,
            ValueTemplate = $"{{{{ value_json.{uniqueId} }}}}",
        };

        if (componentType == HomeAssistantComponent.Switch)
        {
            componentConfiguration.CommandTopic = commandTopic;
        }

        var messagePayload = JsonSerializer.Serialize(componentConfiguration, DefaultJsonSerializerOptions);
        Logger.LogTrace(LogNumbers.MqttService.PublishConfigurationAsyncPayload, $"Payload for configuration of component \"{componentInformation.ComponentId}\": {messagePayload}");

        var configurationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(discoveryMessageTopic)
            .WithPayload(messagePayload)
            .WithRetainFlag(true)
            .Build();

        await MqttClient!.EnqueueAsync(configurationMessage);

        Logger.LogInformation(LogNumbers.MqttService.PublishConfigurationAsyncConfigPublished, $"Published auto discovery message for component \"{componentInformation.ComponentId}\"");
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

            var componentStateTopic = GetStateTopic<T>();
            Logger.LogTrace(LogNumbers.MqttService.SendUpdatesAsyncSensorStateTopic, $"Using topic \"{componentStateTopic}\"");

            // Get all components from the state object. Components are all properties that have a HomeAssistantComponentAttribute.
            var components = updatedEntityType.GetProperties().Where(p => p.GetCustomAttribute<HomeAssistantComponentAttribute>() != null).ToList();
            var componentUpdateMessage = new Dictionary<string, object>();
            foreach (var componentPropertyInfo in components)
            {
                var componentInformation = componentPropertyInfo.GetCustomAttribute<HomeAssistantComponentAttribute>();
                if (componentInformation == null || string.IsNullOrWhiteSpace(componentInformation.ComponentId))
                {
                    Logger.LogWarning(LogNumbers.MqttService.SendUpdatesAsyncSensorInformationEmpty, $"The component \"{componentPropertyInfo.Name}\" has incomplete component information and will be skipped.");
                    continue;
                }

                var entityValue = (bool)(componentPropertyInfo.GetValue(updatedEntity) ?? false);

                var value = entityValue ? "ON" : "OFF";

                componentUpdateMessage.Add(componentInformation.ComponentId, value);
            }

            var statePayload = JsonSerializer.Serialize(componentUpdateMessage, DefaultJsonSerializerOptions);
            Logger.LogTrace(LogNumbers.MqttService.SendUpdatesAsyncPayload, $"Payload for update of \"{updatedEntityType.Name}\": {statePayload}");

            var updateMessage = new MqttApplicationMessageBuilder()
                .WithTopic(componentStateTopic)
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

            // Get all components from the state object. Components are all properties that have a HomeAssistantComponentAttribute.
            var components = stateObjectType.GetProperties().Where(p => p.GetCustomAttribute<HomeAssistantComponentAttribute>() != null).ToList();

            foreach (var componentPropertyInfo in components)
            {
                var componentInformation = componentPropertyInfo.GetCustomAttribute<HomeAssistantComponentAttribute>();
                if (componentInformation == null || string.IsNullOrWhiteSpace(componentInformation.ComponentId))
                {
                    Logger.LogWarning(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncSensorInformationEmpty, $"The component \"{componentPropertyInfo.Name}\" has incomplete component information and will be skipped.");
                    continue;
                }

                var componentType = GetComponentType(componentInformation);

                var discoveryMessageTopic = GetDiscoveryTopic<T>(componentInformation.ComponentId, componentType);
                Logger.LogInformation(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncDiscoveryMessage, $"Discovery topic for component \"{componentPropertyInfo.Name}\": {discoveryMessageTopic}");

                var componentConfigurationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(discoveryMessageTopic)
                    .WithPayload("")
                    .WithRetainFlag(true)
                    .Build();

                await MqttClient.EnqueueAsync(componentConfigurationMessage);
                Logger.LogInformation(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncRemovedSensorConfig, $"Removed component configuration auto discovery message for component \"{componentPropertyInfo.Name}\"");

            }

            Logger.LogTrace(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncSuccess, $"Removed all auto discovery messages for \"{stateObjectType.Name}\"");

            await UnsubscribeFromCommandTopicsAsync();
            MqttClient.ApplicationMessageReceivedAsync -= HandleApplicationMessageReceived;
            CommandTopicToCommandId.Clear();
            Logger.LogTrace(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncUnsubscribedFromTopics, $"Removed subscription to all topics");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.MqttService.RemoveDiscoveryMessageAsyncException, ex, $"An error occurred while removing auto discovery messages for \"{stateObjectType.Name}\": {ex}");
            throw;
        }
    }

    /// <summary>
    /// Subscribe to all command topics
    /// </summary>
    /// <returns></returns>
    public async Task SubscribeToCommandTopicsAsync()
    {
        foreach (var commandTopic in CommandTopicToCommandId.Keys)
        {
            await MqttClient.SubscribeAsync(commandTopic, MqttQualityOfServiceLevel.AtMostOnce);
        }
    }

    /// <summary>
    /// Unsubscribe from all command topics
    /// </summary>
    /// <returns></returns>
    public async Task UnsubscribeFromCommandTopicsAsync()
    {
        foreach (var commandTopic in CommandTopicToCommandId.Keys)
        {
            await MqttClient.UnsubscribeAsync(commandTopic);
        }
    }


    private async Task HandleApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        using var scope = Logger.BeginScope($"{nameof(TeamsCommunication)}:{nameof(HandleApplicationMessageReceived)}");
        try
        {
            var topic = args.ApplicationMessage.Topic;
            Logger.LogTrace(LogNumbers.MqttService.HandleApplicationMessageReceived, $"Received application message from topic {topic}");


            if (!CommandTopicToCommandId.ContainsKey(topic))
            {
                Logger.LogWarning(LogNumbers.MqttService.HandleApplicationMessageReceivedUnrecognizedTopic, $"The topic \"{topic}\" is unrecognized and cannot be matched to a command.");
                return;
            }
            var command = CommandTopicToCommandId[topic];
            var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            Logger.LogInformation(LogNumbers.MqttService.HandleApplicationMessageReceivedReceivedCommand, $"Received command \"{command}\" with payload \"{payload}\" through MQTT.");

            ActionReceived?.Invoke(this, command, payload);

            Logger.LogTrace(LogNumbers.MqttService.HandleApplicationMessageReceivedSuccess, $"Raised event {nameof(ActionReceived)} for command \"{command}\" with payload \"{payload}\".");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.MqttService.HandleApplicationMessageReceivedException, ex, $"An error occurred while handling an incoming application message through MQTT: {ex}");
        }
    }

    public void Dispose()
    {
        MqttClient?.Dispose();
    }

    /// <summary>
    /// Generates the discovery topic for the given component, <paramref name="component"/> and type
    /// </summary>
    /// <param name="componentId">The id of the component for which the discovery topic should be generated</param>
    /// <param name="component">The <see cref="HomeAssistantComponent"/> of the device</param>
    protected string GetDiscoveryTopic<T>(string componentId, HomeAssistantComponent component)
    {
        var typeName = EnsureValidString(typeof(T).Name);
        var validatedComponentId = EnsureValidString(componentId ?? string.Empty);
        var computerName = EnsureValidString(Environment.MachineName);
        var discoveryMessageTopic = new HomeAssistantDiscoveryTopicBuilder()
            .WithDiscoveryPrefix(MqttConfiguration?.HomeAssistantAutoDiscoveryTopic)
            .WithComponent(component)
            .WithObjectId($"Teams2Mqtt-{computerName}-{typeName}-{validatedComponentId}")
            .Build();
        return discoveryMessageTopic;
    }

    /// <summary>
    /// Generates the command topic for the given component
    /// </summary>
    /// <param name="actionId">The id of the action for which the command topic should be generated</param>
    protected string GetCommandTopic<T>(string actionId)
    {
        var typeName = EnsureValidString(typeof(T).Name);
        var validatedActionId = EnsureValidString(actionId ?? string.Empty);
        var computerName = EnsureValidString(Environment.MachineName);

        return $"teams2mqtt/{computerName}-{typeName}/{validatedActionId}/set";
    }


    /// <summary>
    /// Generates the state topic for the given component
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