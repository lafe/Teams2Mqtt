using System.Text.Json.Serialization;

namespace lafe.Teams2Mqtt.Model.Teams;

public class MeetingPermissions
{
    [JsonPropertyName("canToggleMute")]
    public bool CanToggleMute { get; set; }
    [JsonPropertyName("canToggleVideo")]
    public bool CanToggleVideo { get; set; }
    [JsonPropertyName("canToggleHand")]
    public bool CanToggleHand { get; set; }
    [JsonPropertyName("canToggleBlur")]
    public bool CanToggleBlur { get; set; }
    [JsonPropertyName("canToggleRecord")]
    public bool CanToggleRecord { get; set; }
    [JsonPropertyName("canLeave")]
    public bool CanLeave { get; set; }
    [JsonPropertyName("canReact")]
    public bool CanReact { get; set; }
    [JsonPropertyName("canToggleShareTray")]
    public bool CanToggleShareTray { get; set; }
    [JsonPropertyName("canToggleChat")]
    public bool CanToggleChat { get; set; }
    [JsonPropertyName("canStopSharing")]
    public bool CanStopSharing { get; set; }
}