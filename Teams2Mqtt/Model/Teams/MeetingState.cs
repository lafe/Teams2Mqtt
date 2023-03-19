using System.Text.Json.Serialization;
using lafe.Teams2Mqtt.Attributes;
using lafe.Teams2Mqtt.Mqtt;

namespace lafe.Teams2Mqtt.Model.Teams;

public class MeetingState
{
    [HomeAssistantComponent("isMuted", "Is Muted", "isMuted", Icon = "mdi:microphone")]
    [JsonPropertyName("isMuted")]
    public bool IsMuted { get; set; }

    [HomeAssistantComponent("isCameraOn", "Is Camera On", "isCameraOn", Icon = "mdi:webcam")]
    [JsonPropertyName("isCameraOn")]
    public bool IsCameraOn { get; set; }

    [HomeAssistantComponent("isHandRaised", "Is Hand Raised", "isHandRaised", Icon = "mdi:hand-back-left")]
    [JsonPropertyName("isHandRaised")]
    public bool IsHandRaised { get; set; }

    [HomeAssistantBinarySensor("isInMeeting", "Is In Meeting", "isInMeeting", Icon= "mdi:human-greeting-proximity")]
    [JsonPropertyName("isInMeeting")]
    public bool IsInMeeting { get; set; }

    [HomeAssistantComponent("isRecordingOn", "Is Recoding On", "isRecordingOn", Icon="mdi:record-rec")]
    [JsonPropertyName("isRecordingOn")]
    public bool IsRecordingOn { get; set; }

    [HomeAssistantComponent("isBackgroundBlurred", "Is Background Blurred", "IsBackgroundBlurred", Icon="mdi:blur")]
    [JsonPropertyName("isBackgroundBlurred")]
    public bool IsBackgroundBlurred { get; set; }
}