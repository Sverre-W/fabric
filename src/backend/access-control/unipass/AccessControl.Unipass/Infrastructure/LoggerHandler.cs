namespace AccessControl.Unipass.Infrastructure;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

public sealed class JsonLoggingHandler : DelegatingHandler
{
    private readonly ILogger<JsonLoggingHandler> _logger;

    public JsonLoggingHandler(ILogger<JsonLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        await LogRequestAsync(request, cancellationToken);

        var response = await base.SendAsync(request, cancellationToken);

        await LogResponseAsync(response, cancellationToken);

        return response;
    }

    private async Task LogRequestAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
            return;

        if (request.Content is null)
            return;

        var body = await request.Content.ReadAsStringAsync(ct);

        _logger.LogDebug(
            "HTTP Request {Method} {Uri}\n{Body}",
            request.Method,
            request.RequestUri,
            PrettyPrint(body)
        );
    }

    private async Task LogResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
            return;

        if (response.Content is null)
            return;

        var body = await response.Content.ReadAsStringAsync(ct);

        _logger.LogDebug(
            "HTTP Response {StatusCode}\n{Body}",
            (int)response.StatusCode,
            PrettyPrint(body)
        );

        // Rehydrate content so downstream consumers can still read it
        response.Content = new StringContent(
            body,
            Encoding.UTF8,
            response.Content.Headers.ContentType?.MediaType
        );
    }

    private static bool IsJson(string? mediaType) =>
        mediaType != null && mediaType.Contains("json", StringComparison.OrdinalIgnoreCase);

    private static string PrettyPrint(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(
                doc,
                new JsonSerializerOptions { WriteIndented = true }
            );
        }
        catch
        {
            return json; // Not valid JSON, log raw
        }
    }
}
