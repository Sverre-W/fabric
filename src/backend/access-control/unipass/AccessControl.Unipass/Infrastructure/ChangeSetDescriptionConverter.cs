using System.Text.Json;
using System.Text.Json.Serialization;
using AccessControl.Unipass.ChangeSets;

namespace AccessControl.Unipass.Infrastructure;

public sealed class ChangeSetDescriptionConverter : JsonConverter<ChangeSetDescription>
{
    public override ChangeSetDescription Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        throw new NotSupportedException("ChangeSetDescription is write-only.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        ChangeSetDescription value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        // Operation first (explicit and predictable)
        writer.WritePropertyName(nameof(value.Operation));
        JsonSerializer.Serialize(writer, value.Operation.ToString(), options);

        // Flatten properties
        foreach (var kv in value.Properties)
        {
            writer.WritePropertyName(kv.Key);
            JsonSerializer.Serialize(writer, kv.Value, options);
        }

        writer.WriteEndObject();
    }
}
