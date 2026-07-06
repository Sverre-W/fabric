namespace Fabric.Server.Infrastructure;

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

public sealed class NamespaceFallbackJsonTypeInfoResolver(string namespacePrefix) : IJsonTypeInfoResolver
{
    private readonly DefaultJsonTypeInfoResolver _fallback = new();
    private readonly string _namespacePrefix = namespacePrefix;

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        if (IsAllowed(type))
            return _fallback.GetTypeInfo(type, options);

        return null;
    }

    private bool IsAllowed(Type type)
    {
        if (type.Namespace?.StartsWith(_namespacePrefix, StringComparison.Ordinal) == true)
            return true;

        if (type.IsGenericType)
            return type.GetGenericArguments().Any(IsAllowed);

        return type.IsPrimitive
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Guid);
    }
}
