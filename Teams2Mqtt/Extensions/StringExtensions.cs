namespace lafe.Teams2Mqtt.Extensions;

public static class StringExtensions
{
    public static string Pluralize<T>(this string text, IEnumerable<T> collection)
    {
        return collection.Count() == 1 ? text : $"{text}s";
    }

    public static string Pluralize<T>(this string text, IList<T> collection)
    {
        return collection.Count == 1 ? text : $"{text}s";
    }
}