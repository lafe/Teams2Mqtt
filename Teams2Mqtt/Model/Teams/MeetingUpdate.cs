using System.Text.Json.Serialization;

namespace lafe.Teams2Mqtt.Model.Teams;

public class MeetingUpdate
{
    [JsonPropertyName("meetingState")]
    public MeetingState? MeetingState { get; set; }

    [JsonPropertyName("meetingPermissions")]
    public MeetingPermissions? MeetingPermissions { get; set; }
}