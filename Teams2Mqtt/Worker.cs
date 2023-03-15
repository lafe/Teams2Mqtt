using lafe.Teams2Mqtt.Model;
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

        await TeamsCommunication.ConnectAsync(stoppingToken);

        // MqttService is the last one start, because it needs the values from the initialized SmartMonitoring service
        // await MqttService.StartAsync();
        Logger.LogInformation(LogNumbers.Worker.Initialized, "Monitoring services initialized");
        
        var updateInterval = AppConfiguration.UpdateInterval * 1000;
        Logger.LogInformation(LogNumbers.Worker.CalculatedUpdateInterval, $"Using update interval of {updateInterval}ms");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Has to be sent as the last step, because we need the updated values from the services above
                // await MqttService.UpdateAsync();

                // await Task.Delay(updateInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Swallow exception - it is expected when we are terminating
            }
            catch (Exception ex)
            {
                Logger.LogError(LogNumbers.Worker.ExecuteAsyncLoopError, $"An error occurred while updating the service components: {ex}");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = Logger.BeginScope($"{nameof(Worker)}:{nameof(StopAsync)}");
        try
        {
            Logger.LogInformation(LogNumbers.Worker.StopAsync, $"Stop signal received. Stopping all services and removing any registrations.");
            // await MqttService.StopAsync();

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
