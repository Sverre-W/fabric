using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabric.Server.Desfire.Application;

internal static class DesfireJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
