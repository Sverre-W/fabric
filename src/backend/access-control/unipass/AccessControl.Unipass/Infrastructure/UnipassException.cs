using System.Net;

namespace AccessControl.Unipass.Infrastructure;

public sealed class UnipassException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? Content { get; }

    public UnipassException(HttpStatusCode code, string? details)
        : base($"Request failed with status code {(int)code} ({code})")
    {
        StatusCode = code;
        Content = details;
    }
}
