using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace CashFlow.Infraestructure.Handlers;

public class TokenPropagationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenPropagationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader.ToString());
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
