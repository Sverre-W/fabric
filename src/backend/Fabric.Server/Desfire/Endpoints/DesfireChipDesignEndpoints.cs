using System.Text.Json;
using Fabric.Server.Core;
using Fabric.Server.Desfire.Application;
using Fabric.Server.Desfire.Contracts;
using Fabric.Server.Desfire.Domain;
using Fabric.Server.Desfire.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Desfire.Endpoints;

public static class DesfireChipDesignEndpoints
{
    public static IEndpointRouteBuilder MapDesfireChipDesignEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder designs = app.MapGroup("/api/desfire/chip-designs");
        designs.MapGet("", ListChipDesigns).Produces<Page<ChipDesignResponse>>();
        designs.MapPost("", CreateChipDesign).Produces<ChipDesignResponse>(StatusCodes.Status201Created);
        designs.MapGet("/{id:guid}", GetChipDesign).Produces<ChipDesignResponse>().Produces(StatusCodes.Status404NotFound);
        designs.MapPut("/{id:guid}", UpdateChipDesign).Produces<ChipDesignResponse>().Produces(StatusCodes.Status404NotFound);
        designs.MapDelete("/{id:guid}", DeleteChipDesign).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
        return app;
    }

    private static async Task<IResult> ListChipDesigns([AsParameters] BaseListRequest request, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        IPaged<ChipDesign> result = await db.ChipDesigns.AsNoTracking().OrderBy(design => design.Name).ThenByDescending(design => design.Version).GetPageAsync(request.Page, request.PageSize, cancellationToken);
        return Results.Ok(result.Map(design => design.ToResponse()));
    }

    private static async Task<IResult> CreateChipDesign([FromBody] CreateChipDesignRequest request, DesfireDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        int version = request.Version ?? await GetNextVersionAsync(request.Name, db, cancellationToken);
        bool exists = await db.ChipDesigns.AnyAsync(design => design.Name == request.Name && design.Version == version, cancellationToken);
        if (exists)
            return Results.Problem("Chip design version already exists.", statusCode: StatusCodes.Status409Conflict);

        string specificationJson = JsonSerializer.Serialize(request.Specification, DesfireJson.Options);
        ChipDesign design = ChipDesign.Create(request.Name, version, request.Description, specificationJson, timeProvider.GetUtcNow());
        db.ChipDesigns.Add(design);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/desfire/chip-designs/{design.Id}", design.ToResponse());
    }

    private static async Task<IResult> GetChipDesign(Guid id, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        ChipDesign? design = await db.ChipDesigns.AsNoTracking().SingleOrDefaultAsync(design => design.Id == id, cancellationToken);
        return design is null ? Results.NotFound() : Results.Ok(design.ToResponse());
    }

    private static async Task<IResult> UpdateChipDesign(Guid id, [FromBody] UpdateChipDesignRequest request, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        ChipDesign? design = await db.ChipDesigns.SingleOrDefaultAsync(design => design.Id == id, cancellationToken);
        if (design is null)
            return Results.NotFound();

        bool exists = await db.ChipDesigns.AnyAsync(candidate => candidate.Id != id && candidate.Name == request.Name && candidate.Version == request.Version, cancellationToken);
        if (exists)
            return Results.Problem("Chip design version already exists.", statusCode: StatusCodes.Status409Conflict);

        string specificationJson = JsonSerializer.Serialize(request.Specification, DesfireJson.Options);
        design.Update(request.Name, request.Version, request.Description, specificationJson);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(design.ToResponse());
    }

    private static async Task<IResult> DeleteChipDesign(Guid id, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        ChipDesign? design = await db.ChipDesigns.SingleOrDefaultAsync(design => design.Id == id, cancellationToken);
        if (design is null)
            return Results.NotFound();

        bool referenced = await db.Transformations.AnyAsync(transformation => transformation.FromChipDesignName == design.Name || transformation.ToChipDesignName == design.Name, cancellationToken);
        if (referenced)
            return Results.Problem("Cannot delete a chip design referenced by a transformation.", statusCode: StatusCodes.Status409Conflict);

        db.ChipDesigns.Remove(design);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<int> GetNextVersionAsync(string name, DesfireDbContext db, CancellationToken cancellationToken)
    {
        int? current = await db.ChipDesigns.Where(design => design.Name == name).MaxAsync(design => (int?)design.Version, cancellationToken);
        return (current ?? 0) + 1;
    }
}
