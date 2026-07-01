using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fabric.Hardware.Contracts.Agents;
using Fabric.Hardware.Contracts.Commands;
using Fabric.Hardware.Contracts.Events;
using Fabric.Hardware.Contracts.Inventory;

namespace Fabric.Hardware.Agent.Gateway;

public sealed class HardwareGatewayClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task PostHeartbeatAsync(PostHardwareAgentHeartbeatRequest request, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/hardware-agent/heartbeat", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task PostInventoryAsync(PostHardwareInventoryRequest request, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/hardware-agent/inventory", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task PostEventAsync(PostHardwareEventRequest request, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/hardware-agent/events", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<HardwareCommandEnvelope?> GetNextCommandAsync(CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.GetAsync("api/hardware-agent/commands/next", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HardwareCommandEnvelope>(JsonOptions, cancellationToken);
    }

    public async IAsyncEnumerable<Guid> StreamCommandNotificationsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.GetAsync("api/hardware-agent/commands/stream", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!cancellationToken.IsCancellationRequested)
        {
            string? line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
                yield break;

            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                continue;

            string data = line[5..].Trim();
            Guid? commandId = TryReadCommandId(data);
            if (commandId is not null)
                yield return commandId.Value;
        }
    }

    public async Task<HardwareCommandClaimResponse?> ClaimCommandAsync(Guid commandId, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsync($"api/hardware-agent/commands/{commandId}/claim", null, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HardwareCommandClaimResponse>(JsonOptions, cancellationToken);
    }

    public async Task PostCommandResultAsync(Guid commandId, PostHardwareCommandResultRequest request, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync($"api/hardware-agent/commands/{commandId}/result", request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static Guid? TryReadCommandId(string data)
    {
        JsonNode? node = JsonNode.Parse(data);
        string? commandId = node?["commandId"]?.GetValue<string>();
        return Guid.TryParse(commandId, out Guid parsedCommandId) ? parsedCommandId : null;
    }
}
