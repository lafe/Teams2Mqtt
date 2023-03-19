using lafe.Teams2Mqtt.Model;
using lafe.Teams2Mqtt.Model.Teams;
using lafe.Teams2Mqtt.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace lafe.Teams2Mqtt;

public class Worker : BackgroundService, IDisposable
{
    protected ILogger<Worker> Logger { get; }
    protected TeamsCommunication TeamsCommunication { get; }
    protected MqttConfiguration MqttConfiguration { get; }
    protected AppConfiguration AppConfiguration { get; }
    protected MqttService MqttService { get; }
    protected Timer? RefreshStateTimer { get; set; }

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


    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation(LogNumbers.Worker.Initializing, "Initializing monitoring services");

        MqttService.ActionReceived += HomeAssistantActionReceived;
        await MqttService.StartAsync();
        await MqttService.PublishDiscoveryMessagesAsync<MeetingState>(cancellationToken);
        await MqttService.PublishDiscoveryMessagesAsync<MeetingPermissions>(cancellationToken);
        Logger.LogTrace(LogNumbers.Worker.ExecuteAsyncStartedMqtt, $"Started MQTT service");

        TeamsCommunication.ConnectionEstablished += OnConnectionEstablished;
        TeamsCommunication.MeetingUpdateMessageReceived += OnMeetingUpdateMessageReceived;
        TeamsCommunication.ConnectionClosed += OnConnectionClosed;
        Logger.LogTrace(LogNumbers.Worker.ExecuteAsyncAddedEventReceivers, $"Added event receivers for Teams messages");

        await TeamsCommunication.ConnectAsync(cancellationToken);
        Logger.LogInformation(LogNumbers.Worker.Initialized, "Monitoring services initialized");

        RefreshStateTimer = new Timer(RefreshStateTimerCallback, null, TimeSpan.FromSeconds(AppConfiguration.RefreshInterval), TimeSpan.FromMinutes(AppConfiguration.RefreshInterval));
        Logger.LogInformation(LogNumbers.Worker.ExecuteAsyncRefreshStateTimerStarted, $"Refresh state timer started with an interval of {AppConfiguration.RefreshInterval} seconds.");
    }

    private void OnConnectionEstablished(object? sender, EventArgs e)
    {
        Task.Factory.StartNew(async () =>
        {
            Logger.LogTrace(LogNumbers.Worker.OnConnectionEstablishedTeamsOnline, $"Teams connection has been established. Sending online message.");
            await MqttService.SendOnlineAvailabilityMessageAsync();
        });
    }

    private void OnConnectionClosed(object? sender, EventArgs e)
    {
        Task.Factory.StartNew(async () =>
        {
            Logger.LogTrace(LogNumbers.Worker.OnConnectionClosedTeamsOffline, $"Teams connection has been closed. Sending offline message.");
            await MqttService.SendOfflineAvailabilityMessageAsync();
        });
    }

    private void OnMeetingUpdateMessageReceived(object sender, MeetingUpdateMessage e)
    {
        Task.Factory.StartNew(async message =>
        {
            var meetingState = (message as MeetingUpdateMessage)?.MeetingUpdate?.MeetingState;
            if (meetingState == null)
            {
                Logger.LogWarning(LogNumbers.Worker.MeetingUpdateMessageReceivedMeetingStateEmpty, $"The received meeting state object is empty.");
            }
            else
            {
                Logger.LogTrace(LogNumbers.Worker.MeetingUpdateMessageReceivedMeetingStateChangeTriggered, $"Meeting state event has been triggered");
                await MqttService.SendUpdatesAsync(meetingState);
            }

            var meetingPermissions = (message as MeetingUpdateMessage)?.MeetingUpdate?.MeetingPermissions;
            if (meetingPermissions == null)
            {
                Logger.LogWarning(LogNumbers.Worker.MeetingUpdateMessageReceivedMeetingPermissionsEmpty, $"The received meeting permissions object is empty.");
            }
            else
            {
                Logger.LogTrace(LogNumbers.Worker.MeetingUpdateMessageReceivedMeetingPermissionsChangeTriggered, $"Meeting permissions event has been triggered");
                await MqttService.SendUpdatesAsync(meetingPermissions);
            }
        }, e);
    }

    private void RefreshStateTimerCallback(object? state)
    {
        Task.Factory.StartNew(async message =>
        {
            await TeamsCommunication.RequestMeetingStatusAsync();
        }, null);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = Logger.BeginScope($"{nameof(Worker)}:{nameof(StopAsync)}");
        try
        {
            Logger.LogInformation(LogNumbers.Worker.StopAsync, $"Stop signal received. Stopping all services and removing any registrations.");

            if (RefreshStateTimer != null)
            {
                await RefreshStateTimer.DisposeAsync();
                Logger.LogTrace(LogNumbers.Worker.StopAsyncStoppedTimerJob, $"Stopped refresh state timer");
            }

            MqttService.ActionReceived -= HomeAssistantActionReceived;
            await MqttService.RemoveDiscoveryMessageAsync<MeetingState>();
            await MqttService.RemoveDiscoveryMessageAsync<MeetingPermissions>();
            await MqttService.StopAsync();
            Logger.LogTrace(LogNumbers.Worker.StopAsyncStoppedMqttService, $"Stopped MQTT service");

            TeamsCommunication.ConnectionEstablished -= OnConnectionEstablished;
            TeamsCommunication.MeetingUpdateMessageReceived -= OnMeetingUpdateMessageReceived;
            TeamsCommunication.ConnectionClosed -= OnConnectionClosed;
            Logger.LogTrace(LogNumbers.Worker.StopAsyncRemovedEventHandler, $"Unregistered event receivers for Teams messages");

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

    private void HomeAssistantActionReceived(object sender, string actionId, string payload)
    {
        using var scope = Logger.BeginScope($"{nameof(TeamsCommunication)}:{nameof(HomeAssistantActionReceived)}");
        try
        {
            Logger.LogTrace(LogNumbers.Worker.HomeAssistantActionReceived, $"Handling action \"{actionId}\"");

#pragma warning disable CS4014
            switch (actionId)
            {
                case Constants.Commands.BlurBackground:
                    TeamsCommunication.ToggleBackgroundBlurAsync();
                    break;
                case Constants.Commands.RaiseHand:
                    TeamsCommunication.ToggleRaisedHandAsync();
                    break;
                case Constants.Commands.ToggleCamera:
                    TeamsCommunication.ToggleVideoAsync();
                    break;
                case Constants.Commands.ToggleMicrophone:
                    TeamsCommunication.ToggleMuteAsync();
                    break;
            }
#pragma warning restore CS4014
            Logger.LogTrace(LogNumbers.Worker.HomeAssistantActionReceivedSuccess, $"Completed handling of action \"{actionId}\"");
        }
        catch (Exception ex)
        {
            Logger.LogError(LogNumbers.Worker.HomeAssistantActionReceivedException, ex, $"An error occurred while handling the action \"{actionId}\": {ex}");
            throw;
        }
    }
}
