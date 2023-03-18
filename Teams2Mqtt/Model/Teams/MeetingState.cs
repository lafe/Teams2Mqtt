using System.Text.Json.Serialization;
using lafe.Teams2Mqtt.Attributes;
using lafe.Teams2Mqtt.Mqtt;

namespace lafe.Teams2Mqtt.Model.Teams;

public class MeetingState
{
    [Sensor("isMuted", "Is Muted", "isMuted")]
    [JsonPropertyName("isMuted")]
    public bool IsMuted { get; set; }

    [Sensor("isCameraOn", "Is Camera On", "isCameraOn")]
    [JsonPropertyName("isCameraOn")]
    public bool IsCameraOn { get; set; }

    [Sensor("isHandRaised", "Is Hand Raised", "isHandRaised")]
    [JsonPropertyName("isHandRaised")]
    public bool IsHandRaised { get; set; }

    [Sensor("isInMeeting", "Is In Meeting", "isInMeeting")]
    [JsonPropertyName("isInMeeting")]
    public bool IsInMeeting { get; set; }

    [Sensor("isRecordingOn", "Is Recoding On", "isRecordingOn")]
    [JsonPropertyName("isRecordingOn")]
    public bool IsRecordingOn { get; set; }

    [Sensor("isBackgroundBlurred", "Is Background Blurred", "IsBackgroundBlurred")]
    [JsonPropertyName("isBackgroundBlurred")]
    public bool IsBackgroundBlurred { get; set; }
}