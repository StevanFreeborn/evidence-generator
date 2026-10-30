using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace EvidenceGenerator.Api.Auth;

internal sealed class OnspringApiKeyAuthenticationHandler(
  IOptionsMonitor<AuthenticationSchemeOptions> options,
  ILoggerFactory logger,
  UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    if (Request.Headers.TryGetValue(OnspringApiKeyAuthenticationDefaults.HeaderName, out var apiKeyValues) is false)
    {
      return Task.FromResult(AuthenticateResult.NoResult());
    }

    var apiKey = apiKeyValues.FirstOrDefault();

    if (string.IsNullOrWhiteSpace(apiKey))
    {
      return Task.FromResult(AuthenticateResult.Fail($"'{OnspringApiKeyAuthenticationDefaults.HeaderName}' header cannot be empty."));
    }

    var claims = new[]
    {
      new Claim(OnspringApiKeyAuthenticationDefaults.ClaimType, apiKey.Trim())
    };

    var identity = new ClaimsIdentity(claims, Scheme.Name);
    var principal = new ClaimsPrincipal(identity);
    var ticket = new AuthenticationTicket(principal, Scheme.Name);

    return Task.FromResult(AuthenticateResult.Success(ticket));
  }
}