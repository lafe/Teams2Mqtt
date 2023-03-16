using System.Reflection;

namespace lafe.Teams2Mqtt.Extensions;

public static class EnumExtensions
{
    public static TAttribute? GetAttribute<TEnum, TAttribute>(this Enum value)
        where TEnum : Enum
        where TAttribute : Attribute
    {
        var valueType = value.GetType();
        var memberInfo = valueType.GetMember(value.ToString())[0];
        var customAttribute = memberInfo.GetCustomAttribute(typeof(TAttribute), false) as TAttribute;

        return customAttribute;
    }
}