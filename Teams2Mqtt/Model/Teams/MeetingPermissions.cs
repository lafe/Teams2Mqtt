using lafe.Teams2Mqtt.Attributes;
using System.Text.Json.Serialization;

namespace lafe.Teams2Mqtt.Model.Teams;

public class MeetingPermissions
{
    [Sensor("canToggleMute", "Can Toggle Mute", "canToggleMute", EnabledByDefault = false)]
    [JsonPropertyName("canToggleMute")]
    public bool CanToggleMute { get; set; }

    [Sensor("canToggleVideo", "Can Toggle Video", "canToggleVideo", EnabledByDefault = false)]
    [JsonPropertyName("canToggleVideo")]
    public bool CanToggleVideo { get; set; }

    [Sensor("canToggleHand", "Can Toggle Hand", "canToggleHand", EnabledByDefault = false)]
    [JsonPropertyName("canToggleHand")]
    public bool CanToggleHand { get; set; }

    [Sensor("canToggleBlur", "Can Toggle Blur", "canToggleBlur", EnabledByDefault = false)]
    [JsonPropertyName("canToggleBlur")]
    public bool CanToggleBlur { get; set; }

    [Sensor("canToggleRecord", "Can Toggle Recording", "canToggleRecord", EnabledByDefault = false)]
    [JsonPropertyName("canToggleRecord")]
    public bool CanToggleRecord { get; set; }

    [Sensor("canLeave", "Can Leave", "canLeave", EnabledByDefault = false)]
    [JsonPropertyName("canLeave")]
    public bool CanLeave { get; set; }

    [Sensor("canReact", "Can React", "canReact", EnabledByDefault = false)]
    [JsonPropertyName("canReact")]
    public bool CanReact { get; set; }

    [Sensor("canToggleShareTray", "Can Toggle Share Tray", "canToggleShareTray", EnabledByDefault = false)]
    [JsonPropertyName("canToggleShareTray")]
    public bool CanToggleShareTray { get; set; }

    [Sensor("canToggleChat", "Can Toggle Chat", "canToggleChat", EnabledByDefault = false)]
    [JsonPropertyName("canToggleChat")]
    public bool CanToggleChat { get; set; }

    [Sensor("canStopSharing", "Can Stop Sharing", "canStopSharing", EnabledByDefault = false)]
    [JsonPropertyName("canStopSharing")]
    public bool CanStopSharing { get; set; }
}