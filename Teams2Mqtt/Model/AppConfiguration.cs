namespace lafe.Teams2Mqtt.Model;

public class AppConfiguration
{
    /// <summary>
    /// The API Key for the Teams Web Socket
    /// </summary>
    public string? TeamsApiKey { get; set; }

    /// <summary>
    /// The address of the Teams Web Socket endpoint. This is normally "127.0.0.1" or "localhost". The default value is "localhost".
    /// </summary>
    public string TeamsWebSocketAddress { get; set; } = "localhost";

    /// <summary>
    /// The port of the Teams Web Socket endpoint. The default value is 8124.
    /// </summary>
    public int TeamsWebSocketPort { get; set; } = 8124;
}
