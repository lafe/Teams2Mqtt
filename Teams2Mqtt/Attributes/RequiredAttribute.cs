namespace lafe.Teams2Mqtt.Attributes;

/// <summary>
/// Determines that the property is required
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
sealed class RequiredAttribute : Attribute
{
    public RequiredAttribute()
    {
    }
}