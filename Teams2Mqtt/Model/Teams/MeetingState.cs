using System.Text.Json.Serialization;

namespace lafe.Teams2Mqtt.Model.Teams;

public class MeetingState
{
    [JsonPropertyName("isMuted")]
    public bool IsMuted { get; set; }

    [JsonPropertyName("isCameraOn")]
    public bool IsCameraOn { get; set; }

    [JsonPropertyName("isHandRaised")]
    public bool IsHandRaised { get; set; }

    [JsonPropertyName("isInMeeting")]
    public bool IsInMeeting { get; set; }

    [JsonPropertyName("isRecordingOn")]
    public bool IsRecordingOn { get; set; }

    [JsonPropertyName("isBackgroundBlurred")]
    public bool IsBackgroundBlurred { get; set; }
}