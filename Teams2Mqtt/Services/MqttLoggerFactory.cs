using lafe.Teams2Mqtt.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Diagnostics;

namespace lafe.Teams2Mqtt.Services;

public class MqttLoggerFactory
{
    protected ILogger<MqttLoggerFactory> Logger { get; }
    protected ILoggerFactory LoggerFactory { get; }
    protected IOptions<MqttConfiguration> Configuration { get; }

    public MqttLoggerFactory(ILogger<MqttLoggerFactory> logger, ILoggerFactory loggerFactory, IOptions<MqttConfiguration> configuration)
    {
        Logger = logger;
        LoggerFactory = loggerFactory;
        Configuration = configuration;
    }

    public IMqttNetLogger CreateLogger()
    {
        var logger = LoggerFactory.CreateLogger<MqttLogger>();
        return new MqttLogger(logger, Configuration);
    }
}