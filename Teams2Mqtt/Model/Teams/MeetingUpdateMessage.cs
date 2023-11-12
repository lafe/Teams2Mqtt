using System.Globalization;
using System.Text.Json.Serialization;

namespace lafe.Teams2Mqtt.Model.Teams;

public class MeetingUpdateMessage
{
    [JsonPropertyName("apiVersion")]
    public string? ApiVersion { get; set; }

    [JsonPropertyName("meetingUpdate")]
    public MeetingUpdate? MeetingUpdate { get; set; }

    [JsonPropertyName("errorMsg")]
    public string? ErrorMessage { get; set; }
    
    [JsonPropertyName("tokenRefresh")]
    public string? TokenRefresh { get; set; }
}