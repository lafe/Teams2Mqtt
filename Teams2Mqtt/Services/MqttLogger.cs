using lafe.Teams2Mqtt.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Diagnostics;

namespace lafe.Teams2Mqtt.Services;

public class MqttLogger : IMqttNetLogger
{
    public ILogger<MqttLogger> Logger { get; }
    public MqttConfiguration Configuration { get; }

    public MqttLogger(ILogger<MqttLogger> logger, IOptions<MqttConfiguration> configuration)
    {
        Logger = logger;
        Configuration = configuration.Value;
    }

    /// <summary>
    /// Determines if logging is enabled
    /// </summary>
    public bool IsEnabled => Configuration.MqttLoggingEnabled;

    public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
    {
        switch (logLevel)
        {
            case MqttNetLogLevel.Verbose:
                Logger.LogTrace(LogNumbers.MqttLogger.PublishVerbose, exception, $"Source: {source}\nMessage: {message}", parameters);
                break;
            case MqttNetLogLevel.Info:
                Logger.LogTrace(LogNumbers.MqttLogger.PublishInfo, exception, $"Source: {source}\nMessage: {message}", parameters);
                break;
            case MqttNetLogLevel.Warning:
                Logger.LogTrace(LogNumbers.MqttLogger.PublishWarning, exception, $"Source: {source}\nMessage: {message}", parameters);
                break;
            case MqttNetLogLevel.Error:
                Logger.LogTrace(LogNumbers.MqttLogger.PublishError, exception, $"Source: {source}\nMessage: {message}", parameters);
                break;
            default:
                Logger.LogWarning(LogNumbers.MqttLogger.PublishUnknownLevel, $"Logging level \"{logLevel}\" is unknown. Source: {source}\nMessage: {message}");
                break;
        }
    }

}