using Fabric.Server.Core;
using Fabric.Server.Hardware.Application;
using Fabric.Server.Hardware.Contracts;
using Fabric.Server.Hardware.Domain;
using Fabric.Server.Hardware.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Hardware.Endpoints;

public static class HardwareManagementEndpoints
{
    public static IEndpointRouteBuilder MapHardwareManagementEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder hardware = app.MapGroup("/api/hardware");

        hardware.MapGet("/agents", ListHardwareAgents)
            .WithDescription("List hardware agents")
            .WithSummary("List hardware agents")
            .Produces<Page<HardwareAgentResponse>>();
        hardware.MapGet("/agents/{agentId}", GetHardwareAgent)
            .WithDescription("Get hardware agent")
            .WithSummary("Get hardware agent")
            .Produces<HardwareAgentResponse>()
            .Produces(StatusCodes.Status404NotFound);
        hardware.MapPost("/agents", CreateHardwareAgent)
            .WithDescription("Create hardware agent")
            .WithSummary("Create hardware agent")
            .Produces<HardwareAgentKeyResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        hardware.MapPut("/agents/{agentId}", UpdateHardwareAgent)
            .WithDescription("Update hardware agent")
            .WithSummary("Update hardware agent")
            .Produces<HardwareAgentResponse>()
            .Produces(StatusCodes.Status404NotFound);
        hardware.MapPost("/agents/{agentId}/rotate-key", RotateHardwareAgentKey)
            .WithDescription("Rotate hardware agent key")
            .WithSummary("Rotate hardware agent key")
            .Produces<HardwareAgentKeyResponse>()
            .Produces(StatusCodes.Status404NotFound);
        hardware.MapDelete("/agents/{agentId}", DeleteHardwareAgent)
            .WithDescription("Delete hardware agent")
            .WithSummary("Delete hardware agent")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        hardware.MapGet("/agents/{agentId}/devices", ListHardwareDevices)
            .WithDescription("List hardware devices for agent")
            .WithSummary("List hardware devices")
            .Produces<IReadOnlyList<HardwareDeviceResponse>>();
        hardware.MapGet("/agents/{agentId}/devices/{deviceId}", GetHardwareDevice)
            .WithDescription("Get hardware device")
            .WithSummary("Get hardware device")
            .Produces<HardwareDeviceResponse>()
            .Produces(StatusCodes.Status404NotFound);
        hardware.MapGet("/agents/{agentId}/devices/{deviceId}/health", GetHardwareDeviceHealth)
            .WithDescription("Get hardware device health")
            .WithSummary("Get hardware device health")
            .Produces<HardwareDeviceHealthResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListHardwareAgents(
        [AsParameters] BaseListRequest request,
        HardwareConnectionStatusResolver connectionStatusResolver,
        HardwareDbContext db,
        CancellationToken cancellationToken = default)
    {
        IPaged<HardwareAgent> result = await db.Agents
            .AsNoTracking()
            .OrderBy(agent => agent.Name)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Results.Ok(result.Map(agent => agent.ToResponse(connectionStatusResolver.GetStatus(agent.LastSeenAt))));
    }

    private static async Task<IResult> GetHardwareAgent(
        string agentId,
        HardwareConnectionStatusResolver connectionStatusResolver,
        HardwareDbContext db,
        CancellationToken cancellationToken = default)
    {
        string normalizedAgentId = HardwareAgent.NormalizeId(agentId);
        HardwareAgent? agent = await db.Agents
            .AsNoTracking()
            .SingleOrDefaultAsync(agent => agent.Id == normalizedAgentId, cancellationToken);

        return agent is null ? Results.NotFound() : Results.Ok(agent.ToResponse(connectionStatusResolver.GetStatus(agent.LastSeenAt)));
    }

    private static async Task<IResult> CreateHardwareAgent(
        [FromBody] CreateHardwareAgentRequest request,
        HardwareDbContext db,
        HardwareAgentKeyHasher keyHasher,
        CancellationToken cancellationToken = default)
    {
        string agentId = HardwareAgent.NormalizeId(request.Id);
        bool exists = await db.Agents.AnyAsync(agent => agent.Id == agentId, cancellationToken);
        if (exists)
            return Results.Problem("Hardware agent already exists.", statusCode: StatusCodes.Status409Conflict);

        HardwareAgentKey key = keyHasher.CreateKey();
        HardwareAgent agent = HardwareAgent.Create(agentId, request.Name, key.Hash, key.Salt);

        db.Agents.Add(agent);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/hardware/agents/{agent.Id}", new HardwareAgentKeyResponse(agent.ToResponse(HardwareConnectionStatus.Offline), key.Key));
    }

    private static async Task<IResult> UpdateHardwareAgent(
        string agentId,
        [FromBody] UpdateHardwareAgentRequest request,
        HardwareDbContext db,
        CancellationToken cancellationToken = default)
    {
        string normalizedAgentId = HardwareAgent.NormalizeId(agentId);
        HardwareAgent? agent = await db.Agents.SingleOrDefaultAsync(agent => agent.Id == normalizedAgentId, cancellationToken);
        if (agent is null)
            return Results.NotFound();

        agent.Update(request.Name, request.Enabled);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(agent.ToResponse(HardwareConnectionStatus.Offline));
    }

    private static async Task<IResult> RotateHardwareAgentKey(
        string agentId,
        HardwareDbContext db,
        HardwareAgentKeyHasher keyHasher,
        CancellationToken cancellationToken = default)
    {
        string normalizedAgentId = HardwareAgent.NormalizeId(agentId);
        HardwareAgent? agent = await db.Agents.SingleOrDefaultAsync(agent => agent.Id == normalizedAgentId, cancellationToken);
        if (agent is null)
            return Results.NotFound();

        HardwareAgentKey key = keyHasher.CreateKey();
        agent.RotateKey(key.Hash, key.Salt);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(new HardwareAgentKeyResponse(agent.ToResponse(HardwareConnectionStatus.Offline), key.Key));
    }

    private static async Task<IResult> DeleteHardwareAgent(
        string agentId,
        HardwareDbContext db,
        CancellationToken cancellationToken = default)
    {
        string normalizedAgentId = HardwareAgent.NormalizeId(agentId);
        HardwareAgent? agent = await db.Agents.SingleOrDefaultAsync(agent => agent.Id == normalizedAgentId, cancellationToken);
        if (agent is null)
            return Results.NotFound();

        await db.Devices.Where(device => device.AgentId == normalizedAgentId).ExecuteDeleteAsync(cancellationToken);
        await db.EventInbox.Where(item => item.AgentId == normalizedAgentId).ExecuteDeleteAsync(cancellationToken);
        db.Agents.Remove(agent);
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ListHardwareDevices(
        string agentId,
        HardwareConnectionStatusResolver connectionStatusResolver,
        HardwareDbContext db,
        CancellationToken cancellationToken = default)
    {
        string normalizedAgentId = HardwareAgent.NormalizeId(agentId);
        HardwareAgent? agent = await db.Agents
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == normalizedAgentId, cancellationToken);
        HardwareConnectionStatus connectionStatus = connectionStatusResolver.GetStatus(agent?.LastSeenAt);
        HardwareDevice[] devices = await db.Devices
            .AsNoTracking()
            .Where(device => device.AgentId == normalizedAgentId)
            .OrderBy(device => device.DeviceId)
            .ToArrayAsync(cancellationToken);

        return Results.Ok(devices.Select(device => device.ToResponse(connectionStatus, connectionStatusResolver.IsDeviceAvailable(device), connectionStatusResolver.GetDeviceAvailabilityReason(device))).ToArray());
    }

    private static async Task<IResult> GetHardwareDevice(
        string agentId,
        string deviceId,
        HardwareConnectionStatusResolver connectionStatusResolver,
        HardwareDbContext db,
        CancellationToken cancellationToken = default)
    {
        HardwareDevice? device = await GetDeviceAsync(agentId, deviceId, db, cancellationToken);
        if (device is null)
            return Results.NotFound();

        HardwareAgent? agent = await GetAgentAsync(agentId, db, cancellationToken);
        HardwareConnectionStatus connectionStatus = connectionStatusResolver.GetStatus(agent?.LastSeenAt);
        return Results.Ok(device.ToResponse(connectionStatus, connectionStatusResolver.IsDeviceAvailable(device), connectionStatusResolver.GetDeviceAvailabilityReason(device)));
    }

    private static async Task<IResult> GetHardwareDeviceHealth(
        string agentId,
        string deviceId,
        HardwareConnectionStatusResolver connectionStatusResolver,
        HardwareDbContext db,
        CancellationToken cancellationToken = default)
    {
        HardwareDevice? device = await GetDeviceAsync(agentId, deviceId, db, cancellationToken);
        if (device is null)
            return Results.NotFound();

        HardwareAgent? agent = await GetAgentAsync(agentId, db, cancellationToken);
        HardwareConnectionStatus connectionStatus = connectionStatusResolver.GetStatus(agent?.LastSeenAt);
        return Results.Ok(device.ToHealthResponse(connectionStatus, connectionStatusResolver.IsDeviceAvailable(device), connectionStatusResolver.GetDeviceAvailabilityReason(device)));
    }

    private static async Task<HardwareAgent?> GetAgentAsync(
        string agentId,
        HardwareDbContext db,
        CancellationToken cancellationToken)
    {
        string normalizedAgentId = HardwareAgent.NormalizeId(agentId);
        return await db.Agents
            .AsNoTracking()
            .SingleOrDefaultAsync(agent => agent.Id == normalizedAgentId, cancellationToken);
    }

    private static async Task<HardwareDevice?> GetDeviceAsync(
        string agentId,
        string deviceId,
        HardwareDbContext db,
        CancellationToken cancellationToken)
    {
        string normalizedAgentId = HardwareAgent.NormalizeId(agentId);
        string normalizedDeviceId = HardwareDevice.NormalizeDeviceId(deviceId);

        return await db.Devices
            .AsNoTracking()
            .SingleOrDefaultAsync(
                device => device.AgentId == normalizedAgentId && device.DeviceId == normalizedDeviceId,
                cancellationToken);
    }
}
