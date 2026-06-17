using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.AccessPolicies.Endpoints;

internal static class ControllerResultExtensions
{
    public static IResult ToResult(this (int statusCode, ProblemDetails? problemDetails) error) =>
        error.problemDetails is null
            ? Results.StatusCode(error.statusCode)
            : Results.Json(error.problemDetails, statusCode: error.statusCode);
}
