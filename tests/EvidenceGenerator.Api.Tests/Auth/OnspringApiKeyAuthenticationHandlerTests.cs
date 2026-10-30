using System.Text.Encodings.Web;

using EvidenceGenerator.Api.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NSubstitute;

namespace EvidenceGenerator.Api.Tests.Auth;

public class OnspringApiKeyAuthenticationHandlerTests
{
  private readonly IOptionsMonitor<AuthenticationSchemeOptions> _optionsMonitor;
  private readonly UrlEncoder _encoder;

  public OnspringApiKeyAuthenticationHandlerTests()
  {
    _optionsMonitor = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
    _optionsMonitor.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());
    _encoder = UrlEncoder.Default;
  }

  private async Task<AuthenticateResult> RunHandlerAsync(Action<HttpContext>? configureContext = null)
  {
    var handler = new OnspringApiKeyAuthenticationHandler(
        _optionsMonitor,
        NullLoggerFactory.Instance,
        _encoder
    );

    var context = new DefaultHttpContext();
    configureContext?.Invoke(context);

    var scheme = new AuthenticationScheme(
      OnspringApiKeyAuthenticationDefaults.SchemeName,
      OnspringApiKeyAuthenticationDefaults.SchemeName,
      typeof(OnspringApiKeyAuthenticationHandler)
    );

    await handler.InitializeAsync(scheme, context);
    return await handler.AuthenticateAsync();
  }

  [Fact]
  public async Task HandleAuthenticateAsync_MissingHeader_ReturnsNoResult()
  {
    var result = await RunHandlerAsync();

    Assert.False(result.Succeeded);
    Assert.True(result.None);
    Assert.Null(result.Failure);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public async Task HandleAuthenticateAsync_WhitespaceHeader_Fails(string headerValue)
  {
    var result = await RunHandlerAsync(ctx =>
    {
      ctx.Request.Headers[OnspringApiKeyAuthenticationDefaults.HeaderName] = headerValue;
    });

    Assert.False(result.Succeeded);
    Assert.NotNull(result.Failure);
  }

  [Fact]
  public async Task HandleAuthenticateAsync_ValidHeader_SucceedsWithClaim()
  {
    var result = await RunHandlerAsync(ctx =>
    {
      ctx.Request.Headers[OnspringApiKeyAuthenticationDefaults.HeaderName] = "  valid-api-key  ";
    });

    Assert.True(result.Succeeded);
    Assert.NotNull(result.Principal);

    var claim = result.Principal.FindFirst(OnspringApiKeyAuthenticationDefaults.ClaimType);

    Assert.NotNull(claim);
    Assert.Equal("valid-api-key", claim.Value);
  }
}