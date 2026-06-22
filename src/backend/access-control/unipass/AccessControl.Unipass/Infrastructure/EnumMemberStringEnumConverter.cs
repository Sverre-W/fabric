namespace AccessControl.Unipass.Infrastructure;

using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class EnumMemberStringEnumConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    private readonly Dictionary<T, string> _toString;
    private readonly Dictionary<string, T> _fromString;

    public EnumMemberStringEnumConverter()
    {
        _toString = new();
        _fromString = new(StringComparer.OrdinalIgnoreCase);

        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var enumValue = (T)field.GetValue(null)!;

            var name = field.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? field.Name;

            _toString[enumValue] = name;
            _fromString[name] = enumValue;
        }
    }

    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = reader.GetString()!;
        return _fromString.TryGetValue(value, out var result)
            ? result
            : throw new JsonException($"Unknown enum value '{value}' for {typeof(T).Name}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_toString[value]);
    }
}
