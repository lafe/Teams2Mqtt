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
    public TeamsCommunication TeamsCommunication { get; }
    public MqttConfiguration MqttConfiguration { get; }
    public AppConfiguration AppConfiguration { get; }

    public Worker(ILogger<Worker> logger,
        IOptions<AppConfiguration> appConfiguration,
        IOptions<MqttConfiguration> mqttConfiguration,
        TeamsCommunication teamsCommunication)
    {
        Logger = logger;
        TeamsCommunication = teamsCommunication;
        MqttConfiguration = mqttConfiguration.Value;
        AppConfiguration = appConfiguration.Value;
        //MqttService = mqttService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation(LogNumbers.Worker.Initializing, "Initializing monitoring services");

        TeamsCommunication.MeetingUpdateMessageReceived += MeetingUpdateMessageReceived;
        await TeamsCommunication.ConnectAsync(stoppingToken);

        // MqttService is the last one start, because it needs the values from the initialized SmartMonitoring service
        // await MqttService.StartAsync();
        Logger.LogInformation(LogNumbers.Worker.Initialized, "Monitoring services initialized");
    }

    private void MeetingUpdateMessageReceived(object sender, MeetingUpdateMessage e)
    {
        Task.Factory.StartNew(async message =>
        {
            var meetingState = (message as MeetingUpdateMessage)?.MeetingUpdate?.MeetingState;
            Logger.LogInformation($"Meeting started: {meetingState?.IsInMeeting}");
        }, e);
            
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = Logger.BeginScope($"{nameof(Worker)}:{nameof(StopAsync)}");
        try
        {
            Logger.LogInformation(LogNumbers.Worker.StopAsync, $"Stop signal received. Stopping all services and removing any registrations.");
            // await MqttService.StopAsync();

            TeamsCommunication.MeetingUpdateMessageReceived -= MeetingUpdateMessageReceived;

            await TeamsCommunication.DisconnectAsync();
            TeamsCommunication.Dispose();

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
