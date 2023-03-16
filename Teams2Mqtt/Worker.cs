using lafe.Teams2Mqtt.Model;
using lafe.Teams2Mqtt.Model.Teams;
using lafe.Teams2Mqtt.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace lafe.Teams2Mqtt;

public class Worker : BackgroundService
{
    protected ILogger<Worker> Logger { get; }
    protected TeamsCommunication TeamsCommunication { get; }
    protected MqttConfiguration MqttConfiguration { get; }
    protected AppConfiguration AppConfiguration { get; }
    protected MqttService MqttService { get; }

    public Worker(ILogger<Worker> logger,
        IOptions<AppConfiguration> appConfiguration,
        IOptions<MqttConfiguration> mqttConfiguration,
        TeamsCommunication teamsCommunication,
        MqttService mqttService)
    {
        Logger = logger;
        TeamsCommunication = teamsCommunication;
        MqttConfiguration = mqttConfiguration.Value;
        AppConfiguration = appConfiguration.Value;
        MqttService = mqttService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation(LogNumbers.Worker.Initializing, "Initializing monitoring services");

        TeamsCommunication.MeetingUpdateMessageReceived += MeetingUpdateMessageReceived;
        await TeamsCommunication.ConnectAsync(stoppingToken);
        await MqttService.StartAsync();
        await MqttService.PublishDiscoveryMessagesAsync<MeetingState>();

        Logger.LogInformation(LogNumbers.Worker.Initialized, "Monitoring services initialized");
    }

    private void MeetingUpdateMessageReceived(object sender, MeetingUpdateMessage e)
    {
        Task.Factory.StartNew(async message =>
        {
            var meetingState = (message as MeetingUpdateMessage)?.MeetingUpdate?.MeetingState;
            if (meetingState == null)
            {
                Logger.LogWarning(LogNumbers.Worker.MeetingUpdateMessageReceivedMeetingStateEmpty, $"The received meeting state object is empty.");
                return;
            }

            Logger.LogTrace(LogNumbers.Worker.MeetingUpdateMessageReceivedMeetingStateChangeTriggered, $"Meeting state event has been triggered");
            await MqttService.SendUpdatesAsync(meetingState);
        }, e);
            
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = Logger.BeginScope($"{nameof(Worker)}:{nameof(StopAsync)}");
        try
        {
            Logger.LogInformation(LogNumbers.Worker.StopAsync, $"Stop signal received. Stopping all services and removing any registrations.");
            await MqttService.RemoveDiscoveryMessageAsync<MeetingState>();
            await MqttService.StopAsync();
            Logger.LogTrace(LogNumbers.Worker.StopAsyncStoppedMqttService, $"Stopped MQTT service");

            TeamsCommunication.MeetingUpdateMessageReceived -= MeetingUpdateMessageReceived;
            Logger.LogTrace(LogNumbers.Worker.StopAsyncRemovedEventHandler, $"Unregistered event receiver for Teams messages");

            await TeamsCommunication.DisconnectAsync();
            Logger.LogTrace(LogNumbers.Worker.StopAsyncDisconnectedTeams, $"Disconnected Teams connection");
            TeamsCommunication.Dispose();
            Logger.LogTrace(LogNumbers.Worker.StopAsyncDisposedTeams, $"Disposed Team connection");

            await base.StopAsync(cancellationToken);
            Logger.LogInformation(LogNumbers.Worker.StopAsyncSuccess, $"Stopped all services.");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.Worker.StopAsyncException, ex, $"An error occurred while stopping all services: {ex}");
            throw;
        }
    }
}
