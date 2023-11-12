using System.Text.Json.Serialization;
using lafe.Teams2Mqtt.Attributes;
using lafe.Teams2Mqtt.Mqtt;

namespace lafe.Teams2Mqtt.Model.Teams;

public class MeetingState
{
    [HomeAssistantSwitch("isMuted", Constants.Commands.ToggleMicrophone, "Is Muted", "isMuted", Icon = "mdi:microphone")]
    [JsonPropertyName("isMuted")]
    public bool IsMuted { get; set; }

    [HomeAssistantSwitch("isCameraOn", Constants.Commands.ToggleCamera, "Is Video On", "isCameraOn", Icon = "mdi:webcam")]
    [JsonPropertyName("isVideoOn")]
    public bool IsCameraOn { get; set; }

    [HomeAssistantSwitch("isHandRaised", Constants.Commands.RaiseHand, "Is Hand Raised", "isHandRaised", Icon = "mdi:hand-back-left")]
    [JsonPropertyName("isHandRaised")]
    public bool IsHandRaised { get; set; }

    [HomeAssistantBinarySensor("isInMeeting", "Is In Meeting", "isInMeeting", Icon= "mdi:human-greeting-proximity")]
    [JsonPropertyName("isInMeeting")]
    public bool IsInMeeting { get; set; }

    [HomeAssistantBinarySensor("isRecordingOn", "Is Recoding On", "isRecordingOn", Icon="mdi:record-rec")]
    [JsonPropertyName("isRecordingOn")]
    public bool IsRecordingOn { get; set; }

    [HomeAssistantSwitch("isBackgroundBlurred", Constants.Commands.BlurBackground, "Is Background Blurred", "IsBackgroundBlurred", Icon="mdi:blur")]
    [JsonPropertyName("isBackgroundBlurred")]
    public bool IsBackgroundBlurred { get; set; }

    [HomeAssistantSwitch("isSharing", "Is Sharing", "IsSharing", Icon="mdi:share")]
    [JsonPropertyName("isSharing")]
    public bool IsSharing { get; set; }

    [HomeAssistantSwitch("hasUnreadMessages", "Has unread messages", "HasUnreadMessages", Icon= "mdi:message-badge")]
    [JsonPropertyName("hasUnreadMessages")]
    public bool HasUnreadMessages { get; set; }
}