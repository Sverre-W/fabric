using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.Core;

public static class ResultExtensions
{
    public static IResult AsResponse<TOk, TError>(this Result<TOk, TError> result,
        Func<TError, (int, ProblemDetails?)> mapError)
    {
        return result.Match(
            Results.Ok,
            error =>
            {
                (int statusCode, ProblemDetails? problemDetails) = mapError(error);
                return problemDetails is not null ? Results.Json(problemDetails, statusCode: statusCode) : Results.StatusCode(statusCode);
            }
        );
    }


    public static IResult AsResponse<TError>(this Result<TError> result,
        Func<TError, (int, ProblemDetails?)> mapError)
    {
        return result.Match(
            Results.NoContent,
            error =>
            {
                (int statusCode, ProblemDetails? problemDetails) = mapError(error);

                if (problemDetails is not null)
                {
                    return Results.Json(problemDetails, statusCode: statusCode);
                }

                return Results.StatusCode(statusCode);
            }
        );
    }

}
