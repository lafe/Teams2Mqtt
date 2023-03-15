namespace lafe.Teams2Mqtt;

public class AppConfiguration
{
    /// <summary>
    /// The API Key for the Teams Web Socket
    /// </summary>
    public string? TeamsApiKey { get; set; }

    /// <summary>
    /// The interval in seconds at which the fan speeds should be updated.
    /// </summary>
    public int UpdateInterval { get; set; } = 5;
}
