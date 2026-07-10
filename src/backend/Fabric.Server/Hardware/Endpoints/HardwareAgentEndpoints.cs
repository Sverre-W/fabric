using System.Threading.Channels;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fabric.Hardware.Contracts.Agents;
using Fabric.Hardware.Contracts.Commands;
using Fabric.Hardware.Contracts.Events;
using Fabric.Hardware.Contracts.Inventory;
using Fabric.Server.Hardware.Application;
using Fabric.Server.Hardware.Domain;
using Fabric.Server.Hardware.Persistence;
using Fabric.Server.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Hardware.Endpoints;

public static class HardwareAgentEndpoints
{
    private static readonly JsonSerializerOptions StreamJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static IEndpointRouteBuilder MapHardwareAgentEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder agents = app.MapGroup("/api/hardware-agent")
            .RequireAuthorization(HardwareAgentAuthenticationDefaults.Policy);

        agents.MapGet("/commands/stream", StreamCommands)
            .WithDescription("Open hardware agent command notification stream")
            .WithSummary("Stream hardware commands")
            .Produces(StatusCodes.Status200OK);
        agents.MapGet("/commands/next", GetNextCommand)
            .WithDescription("Get next pending command for hardware agent")
            .WithSummary("Get next hardware command")
            .Produces<HardwareCommandEnvelope>()
            .Produces(StatusCodes.Status204NoContent);
        agents.MapPost("/commands/{commandId:guid}/claim", ClaimCommand)
            .WithDescription("Claim a pending hardware command")
            .WithSummary("Claim hardware command")
            .Produces<HardwareCommandClaimResponse>()
            .Produces(StatusCodes.Status404NotFound);
        agents.MapGet("/commands/{commandId:guid}/status", GetCommandStatus)
            .WithDescription("Get hardware command status")
            .WithSummary("Get hardware command status")
            .Produces<HardwareCommandStatusResponse>()
            .Produces(StatusCodes.Status404NotFound);
        agents.MapPost("/commands/{commandId:guid}/result", PostCommandResult)
            .WithDescription("Post hardware command result")
            .WithSummary("Post hardware command result")
            .Produces(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status404NotFound);
        agents.MapPost("/inventory", PostInventory)
            .WithDescription("Report hardware agent inventory")
            .WithSummary("Report hardware inventory")
            .Produces(StatusCodes.Status204NoContent);
        agents.MapPost("/events", PostEvent)
            .WithDescription("Post hardware event")
            .WithSummary("Post hardware event")
            .Produces(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status404NotFound);
        agents.MapPost("/heartbeat", PostHeartbeat)
            .WithDescription("Post hardware agent heartbeat")
            .WithSummary("Post hardware heartbeat")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task StreamCommands(
        HttpContext context,
        HardwareAgentConnectionManager connectionManager,
        CancellationToken cancellationToken = default)
    {
        string agentId = context.User.GetAgentId();
        context.Response.Headers.ContentType = "text/event-stream";

        ChannelReader<HardwareCommandStreamEvent> reader = connectionManager.Connect(agentId);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var commandCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                using var heartbeatCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                Task<bool> commandAvailable = reader.WaitToReadAsync(commandCancellation.Token).AsTask();
                Task heartbeatDelay = Task.Delay(TimeSpan.FromSeconds(15), heartbeatCancellation.Token);
                Task completed = await Task.WhenAny(commandAvailable, heartbeatDelay);

                if (completed == heartbeatDelay)
                {
                    await commandCancellation.CancelAsync();
                    await context.Response.WriteAsync(": heartbeat\n\n", cancellationToken);
                    await context.Response.Body.FlushAsync(cancellationToken);
                    continue;
                }

                await heartbeatCancellation.CancelAsync();

                if (!await commandAvailable)
                    break;

                while (reader.TryRead(out HardwareCommandStreamEvent? commandEvent))
                {
                    if (commandEvent is null)
                        continue;

                    await context.Response.WriteAsync($"event: {ToEventName(commandEvent.Type)}\n", cancellationToken);
                    await context.Response.WriteAsync($"data: {JsonSerializer.Serialize(commandEvent, StreamJsonOptions)}\n\n", cancellationToken);
                    await context.Response.Body.FlushAsync(cancellationToken);
                }
            }
        }
        finally
        {
            connectionManager.Disconnect(agentId);
        }
    }

    private static IResult GetNextCommand(
        HttpContext context,
        HardwareCommandStore commandStore)
    {
        string agentId = context.User.GetAgentId();
        PendingHardwareCommand? command = commandStore.GetNext(agentId);

        return command is null ? Results.NoContent() : Results.Ok(command.ToEnvelope());
    }

    private static IResult ClaimCommand(
        Guid commandId,
        HttpContext context,
        HardwareCommandStore commandStore)
    {
        string agentId = context.User.GetAgentId();
        return commandStore.TryClaim(commandId, agentId, out HardwareCommandClaimResponse? response)
            ? Results.Ok(response)
            : Results.NotFound();
    }

    private static IResult GetCommandStatus(
        Guid commandId,
        HttpContext context,
        HardwareCommandStore commandStore)
    {
        string agentId = context.User.GetAgentId();
        HardwareCommandStatusResponse? response = commandStore.GetStatus(commandId, agentId);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static IResult PostCommandResult(
        Guid commandId,
        [FromBody] PostHardwareCommandResultRequest request,
        HttpContext context,
        HardwareCommandStore commandStore)
    {
        string agentId = context.User.GetAgentId();
        return commandStore.TryComplete(commandId, agentId, request)
            ? Results.Accepted()
            : Results.NotFound();
    }

    private static async Task<IResult> PostInventory(
        [FromBody] PostHardwareInventoryRequest request,
        HttpContext context,
        HardwareDbContext db,
        CancellationToken cancellationToken = default)
    {
        string agentId = context.User.GetAgentId();
        HardwareAgent? agent = await db.Agents.SingleOrDefaultAsync(agent => agent.Id == agentId, cancellationToken);
        if (agent is null)
            return Results.NotFound();

        agent.MarkInventoryReported(request.ReportedAt);

        List<HardwareDevice> existingDevices = await db.Devices
            .Where(device => device.AgentId == agentId)
            .ToListAsync(cancellationToken);
        HashSet<string> reportedDeviceIds = request.Devices
            .Select(device => HardwareDevice.NormalizeDeviceId(device.DeviceId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (HardwareDeviceInventoryItem item in request.Devices)
        {
            string deviceId = HardwareDevice.NormalizeDeviceId(item.DeviceId);
            string diagnosticsJson = System.Text.Json.JsonSerializer.Serialize(
                item.Diagnostics,
                HardwareJsonSerializerContext.Default.HardwareDeviceDiagnostics);

            HardwareDevice? existingDevice = existingDevices.SingleOrDefault(device => device.DeviceId == deviceId);
            if (existingDevice is null)
            {
                db.Devices.Add(HardwareDevice.Create(
                    agentId,
                    deviceId,
                    item.Kind,
                    item.Driver,
                    item.Capabilities,
                    item.State,
                    diagnosticsJson,
                    request.ReportedAt));
                continue;
            }

            existingDevice.UpdateInventory(
                item.Kind,
                item.Driver,
                item.Capabilities,
                item.State,
                diagnosticsJson,
                request.ReportedAt);
        }

        foreach (HardwareDevice staleDevice in existingDevices.Where(device => !reportedDeviceIds.Contains(device.DeviceId)))
            db.Devices.Remove(staleDevice);

        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> PostEvent(
        [FromBody] PostHardwareEventRequest request,
        HttpContext context,
        HardwareDbContext db,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default)
    {
        string agentId = context.User.GetAgentId();
        string deviceId = HardwareDevice.NormalizeDeviceId(request.DeviceId);
        bool deviceExists = await db.Devices.AnyAsync(
            device => device.AgentId == agentId && device.DeviceId == deviceId,
            cancellationToken);

        if (!deviceExists)
            return Results.NotFound();

        bool alreadyReceived = await db.EventInbox.AnyAsync(item => item.EventId == request.EventId, cancellationToken);
        if (!alreadyReceived)
        {
            db.EventInbox.Add(HardwareEventInboxItem.Create(
                request.EventId,
                agentId,
                deviceId,
                request.Type,
                request.OccurredAt,
                request.Payload,
                timeProvider.GetUtcNow()));
            await db.SaveChangesAsync(cancellationToken);
        }

        return Results.Accepted();
    }

    private static async Task<IResult> PostHeartbeat(
        [FromBody] PostHardwareAgentHeartbeatRequest request,
        HttpContext context,
        HardwareDbContext db,
        CancellationToken cancellationToken = default)
    {
        string agentId = context.User.GetAgentId();
        HardwareAgent? agent = await db.Agents.SingleOrDefaultAsync(agent => agent.Id == agentId, cancellationToken);
        if (agent is null)
            return Results.NotFound();

        agent.MarkSeen(request.ReportedAt);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static string ToEventName(HardwareCommandEventType eventType) => eventType switch
    {
        HardwareCommandEventType.CommandAvailable => "command-available",
        HardwareCommandEventType.CommandCancelled => "command-cancelled",
        _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
    };
}
