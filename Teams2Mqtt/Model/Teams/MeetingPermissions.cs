using lafe.Teams2Mqtt.Attributes;
using System.Text.Json.Serialization;

namespace lafe.Teams2Mqtt.Model.Teams;

public class MeetingPermissions
{
    [Sensor("canToggleMute", "Can Toggle Mute", "canToggleMute")]
    [JsonPropertyName("canToggleMute")]
    public bool CanToggleMute { get; set; }

    [Sensor("canToggleVideo", "Can Toggle Video", "canToggleVideo")]
    [JsonPropertyName("canToggleVideo")]
    public bool CanToggleVideo { get; set; }

    [Sensor("canToggleHand", "Can Toggle Hand", "canToggleHand")]
    [JsonPropertyName("canToggleHand")]
    public bool CanToggleHand { get; set; }

    [Sensor("canToggleBlur", "Can Toggle Blur", "canToggleBlur")]
    [JsonPropertyName("canToggleBlur")]
    public bool CanToggleBlur { get; set; }

    [Sensor("canToggleRecord", "Can Toggle Recording", "canToggleRecord")]
    [JsonPropertyName("canToggleRecord")]
    public bool CanToggleRecord { get; set; }

    [Sensor("canLeave", "Can Leave", "canLeave")]
    [JsonPropertyName("canLeave")]
    public bool CanLeave { get; set; }

    [Sensor("canReact", "Can React", "canReact")]
    [JsonPropertyName("canReact")]
    public bool CanReact { get; set; }

    [Sensor("canToggleShareTray", "Can Toggle Share Tray", "canToggleShareTray")]
    [JsonPropertyName("canToggleShareTray")]
    public bool CanToggleShareTray { get; set; }

    [Sensor("canToggleChat", "Can Toggle Chat", "canToggleChat")]
    [JsonPropertyName("canToggleChat")]
    public bool CanToggleChat { get; set; }

    [Sensor("canStopSharing", "Can Stop Sharing", "canStopSharing")]
    [JsonPropertyName("canStopSharing")]
    public bool CanStopSharing { get; set; }
}