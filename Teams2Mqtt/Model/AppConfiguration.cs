namespace lafe.Teams2Mqtt.Model;

public class AppConfiguration
{
    /// <summary>
    /// The address of the Teams Web Socket endpoint. This is normally "127.0.0.1" or "localhost". The default value is "localhost".
    /// </summary>
    public string TeamsWebSocketAddress { get; set; } = "localhost";

    /// <summary>
    /// The port of the Teams Web Socket endpoint. The default value is 8124.
    /// </summary>
    public int TeamsWebSocketPort { get; set; } = 8124;

    /// <summary>
    /// The interval in seconds to wait before trying to reconnect to the Teams Web Socket endpoint. The default value is 10.
    /// </summary>
    public int TeamsReconnectInterval { get; set; } = 10;

    /// <summary>
    /// The interval in seconds to wait before refreshing the state of the sensors. The default value is 30 seconds.
    /// </summary>
    public int RefreshInterval { get; set; } = 30;
}
