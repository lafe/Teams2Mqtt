namespace lafe.Teams2Mqtt.Attributes;

/// <summary>
/// Determines that the property is optional
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
sealed class OptionalAttribute : Attribute
{
    public OptionalAttribute()
    {
    }
}