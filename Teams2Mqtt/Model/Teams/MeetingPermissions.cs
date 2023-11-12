using lafe.Teams2Mqtt.Attributes;
using System.Text.Json.Serialization;

namespace lafe.Teams2Mqtt.Model.Teams;

public class MeetingPermissions
{
    [HomeAssistantComponent("canToggleMute", "Can Toggle Mute", "canToggleMute", EnabledByDefault = false)]
    [JsonPropertyName("canToggleMute")]
    public bool CanToggleMute { get; set; }

    [HomeAssistantComponent("canToggleVideo", "Can Toggle Video", "canToggleVideo", EnabledByDefault = false)]
    [JsonPropertyName("canToggleVideo")]
    public bool CanToggleVideo { get; set; }

    [HomeAssistantComponent("canToggleHand", "Can Toggle Hand", "canToggleHand", EnabledByDefault = false)]
    [JsonPropertyName("canToggleHand")]
    public bool CanToggleHand { get; set; }

    [HomeAssistantComponent("canToggleBlur", "Can Toggle Blur", "canToggleBlur", EnabledByDefault = false)]
    [JsonPropertyName("canToggleBlur")]
    public bool CanToggleBlur { get; set; }

    [HomeAssistantComponent("canToggleRecord", "Can Toggle Recording", "canToggleRecord", EnabledByDefault = false)]
    [JsonPropertyName("canToggleRecord")]
    public bool CanToggleRecord { get; set; }

    [HomeAssistantComponent("canLeave", "Can Leave", "canLeave", EnabledByDefault = false)]
    [JsonPropertyName("canLeave")]
    public bool CanLeave { get; set; }

    [HomeAssistantComponent("canReact", "Can React", "canReact", EnabledByDefault = false)]
    [JsonPropertyName("canReact")]
    public bool CanReact { get; set; }

    [HomeAssistantComponent("canToggleShareTray", "Can Toggle Share Tray", "canToggleShareTray", EnabledByDefault = false)]
    [JsonPropertyName("canToggleShareTray")]
    public bool CanToggleShareTray { get; set; }

    [HomeAssistantComponent("canToggleChat", "Can Toggle Chat", "canToggleChat", EnabledByDefault = false)]
    [JsonPropertyName("canToggleChat")]
    public bool CanToggleChat { get; set; }

    [HomeAssistantComponent("canStopSharing", "Can Stop Sharing", "canStopSharing", EnabledByDefault = false)]
    [JsonPropertyName("canStopSharing")]
    public bool CanStopSharing { get; set; }

    [HomeAssistantComponent("canPair", "Can Pair", "canPair", EnabledByDefault = false)]
    [JsonPropertyName("canPair")]
    public bool CanPair { get; set; }
}