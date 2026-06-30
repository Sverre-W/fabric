using Fabric.Hardware.Contracts;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Labels;
using Fabric.Hardware.Contracts.Qr;
using Fabric.Server.Hardware.Application;
using Fabric.Server.Hardware.Domain;
using Fabric.Server.Hardware.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Hardware.Endpoints;

public static class HardwareOperationEndpoints
{
    public static IEndpointRouteBuilder MapHardwareOperationEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder operations = app.MapGroup("/api/hardware/agents/{agentId}/devices/{deviceId}");

        operations.MapPost("/qr/scan", ScanQr)
            .WithDescription("Scan QR using a hardware device")
            .WithSummary("Scan QR")
            .Produces<QrScanResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        operations.MapPost("/labels/print", PrintLabel)
            .WithDescription("Print label using a hardware device")
            .WithSummary("Print label")
            .Produces<LabelPrintResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> ScanQr(
        string agentId,
        string deviceId,
        HardwareDbContext db,
        IQrScanner qrScanner,
        CancellationToken cancellationToken = default)
    {
        IResult? validationFailure = await ValidateDeviceAsync(agentId, deviceId, HardwareCapabilities.QrScan, db, cancellationToken);
        if (validationFailure is not null)
            return validationFailure;

        QrScanResponse response = await qrScanner.ScanAsync(new HardwareDeviceRef(agentId, deviceId), cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> PrintLabel(
        string agentId,
        string deviceId,
        [FromBody] PrintLabelRequest request,
        HardwareDbContext db,
        ILabelPrinter labelPrinter,
        CancellationToken cancellationToken = default)
    {
        IResult? validationFailure = await ValidateDeviceAsync(agentId, deviceId, HardwareCapabilities.LabelPrint, db, cancellationToken);
        if (validationFailure is not null)
            return validationFailure;

        LabelPrintResponse response = await labelPrinter.PrintAsync(new HardwareDeviceRef(agentId, deviceId), request, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult?> ValidateDeviceAsync(
        string agentId,
        string deviceId,
        string capability,
        HardwareDbContext db,
        CancellationToken cancellationToken)
    {
        string normalizedAgentId = HardwareAgent.NormalizeId(agentId);
        string normalizedDeviceId = HardwareDevice.NormalizeDeviceId(deviceId);

        HardwareDevice? device = await db.Devices
            .AsNoTracking()
            .SingleOrDefaultAsync(
                device => device.AgentId == normalizedAgentId && device.DeviceId == normalizedDeviceId,
                cancellationToken);

        if (device is null)
            return Results.NotFound();

        if (!device.Enabled)
            return Results.Problem("Hardware device is disabled.", statusCode: StatusCodes.Status409Conflict);

        if (!device.Capabilities.Contains(capability, StringComparer.OrdinalIgnoreCase))
            return Results.Problem("Hardware device does not support requested capability.", statusCode: StatusCodes.Status409Conflict);

        return null;
    }
}
