using lafe.Teams2Mqtt.Attributes;

namespace lafe.Teams2Mqtt.Mqtt;

public enum HomeAssistantComponent
{
    [Name("alarm_control_panel")]
    AlarmControlPanel,
    [Name("binary_sensor")]
    BinarySensor,
    [Name("button")]
    Button,
    [Name("camera")]
    Camera,
    [Name("cover")]
    Cover,
    [Name("device_tracker")]
    DeviceTracker,
    [Name("fan")]
    Fan,
    [Name("humidifier")]
    Humidifier,
    [Name("climate")]
    Climate,
    [Name("light")]
    Light,
    [Name("lock")]
    Lock,
    [Name("number")]
    Number,
    [Name("scene")]
    Scene,
    [Name("select")]
    Select,
    [Name("sensor")]
    Sensor,
    [Name("siren")]
    Siren,
    [Name("switch")]
    Switch,
    [Name("tag")]
    Tag,
    [Name("text")]
    Text,
    [Name("vacuum")]
    Vacuum,
}