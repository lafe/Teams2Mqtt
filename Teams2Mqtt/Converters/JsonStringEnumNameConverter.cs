using lafe.Teams2Mqtt.Attributes;
using lafe.Teams2Mqtt.Extensions;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace lafe.Teams2Mqtt.Converters;

public class JsonStringEnumNameConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, System.Enum
{
    private readonly Dictionary<TEnum, string> _enumToString = new();

    public JsonStringEnumNameConverter()
    {
        var type = typeof(TEnum);

        foreach (var value in Enum.GetValues<TEnum>())
        {
            var nameAttribute = value.GetAttribute<TEnum, NameAttribute>();
            var name = nameAttribute?.Name ?? value.ToString();
            _enumToString.Add(value, name);
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_enumToString[value]);
    }
}