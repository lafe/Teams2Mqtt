using System.Text.Json.Serialization;

namespace lafe.Teams2Mqtt.Model.Teams;

public class TeamsAction
{
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("parameters")] 
    public TeamsActionParameters Parameters { get; set; } = new TeamsActionParameters();

    protected static object RequestIdLock = new object();
    protected static int InternalRequestId = 0;
    [JsonPropertyName("requestId")]
    public int RequestId => InternalRequestId;

    public TeamsAction(string action)
    {
        Action = action;
        lock (RequestIdLock)
        {
            InternalRequestId++;
        }
    }

    public TeamsAction(string action, string actionType)
    : this(action)
    {
        Parameters.ActionType = actionType;
    }
}

public class TeamsActionParameters
{

    [JsonPropertyName("type")]
    public string? ActionType { get; set; }

    public TeamsActionParameters()
    {
    }

    public TeamsActionParameters(string actionType)
    {
        ActionType = actionType;
    }

}