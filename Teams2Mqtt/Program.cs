using lafe.Teams2Mqtt.Model;
using lafe.Teams2Mqtt.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using MQTTnet.Diagnostics;

namespace lafe.Teams2Mqtt
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostingContext, builder) =>
            {
                builder.ClearProviders();
                builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                builder.AddConfiguration(hostingContext.Configuration.GetSection("EventLog"));
                builder.AddConsole();
                builder.AddDebug();
                if (OperatingSystem.IsWindows())
                {
                    builder.AddEventLog(new EventLogSettings
                    {
                        SourceName = "Teams2Mqtt",
                        LogName = "Teams2Mqtt"
                    });
                }
            })
            .ConfigureServices((hostingContext, services) =>
            {
                LoadConfiguration(services, hostingContext.Configuration);
                RegisterServices(services);
                AddHostedServices(services);
            })
            .UseWindowsService(options =>
            {
                options.ServiceName = "Teams2Mqtt";
            })
            .Build();

            var logger = host.Services.GetService<ILogger<Program>>();
            logger?.LogInformation(LogNumbers.Program.InitializationComplete, "Initialization complete");

            await host.RunAsync();
        }


        private static void LoadConfiguration(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);

            var appConfig = configuration.GetSection("configuration");
            services.Configure<AppConfiguration>(appConfig);
            var mqttConfig = configuration.GetSection("mqtt");
            services.Configure<MqttConfiguration>(mqttConfig);
            var sensorLocalizations = configuration.GetSection("sensorLocalizations");
            services.Configure<List<SensorLocalizationConfiguration>>(sensorLocalizations);
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddDataProtection();

            services.AddSingleton<TeamsCommunication>();
            services.AddSingleton<MqttService>();
            services.AddSingleton<ITokenManager>(provider =>
            {
                var logger = provider.GetService<ILogger<TokenManager>>();
                var dataProtectionProvider = provider.GetService<IDataProtectionProvider> ();
                var tokenManager = new TokenManager(logger, dataProtectionProvider);
                tokenManager.Initialize();
                return tokenManager;
            });

            services.AddTransient<MqttLoggerFactory>();
        }

        private static void AddHostedServices(IServiceCollection services)
        {
            services.AddHostedService<Worker>();
        }
    }
}