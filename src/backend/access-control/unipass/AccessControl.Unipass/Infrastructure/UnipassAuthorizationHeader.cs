namespace AccessControl.Unipass.Infrastructure;

public class UnipassAuthorizationHeader(string apiKey) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("Authorization", apiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
