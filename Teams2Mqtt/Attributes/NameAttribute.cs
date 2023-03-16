namespace lafe.Teams2Mqtt.Attributes;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
sealed class NameAttribute : Attribute
{
    public string Name { get; private set; }

    public NameAttribute(string name)
    {
        Name = name;
    }
}