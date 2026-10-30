using System.Net;

namespace EvidenceGenerator.Api.Tests.Endpoints;

public class DocumentationEndpointTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
  private readonly CustomWebApplicationFactory _factory = factory;

  [Fact]
  public async Task Get_RootPath_Returns200AndServesDocumentationHtml()
  {
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(response.Content.Headers.ContentType);
    Assert.Contains("text/html", response.Content.Headers.ContentType.MediaType, StringComparison.OrdinalIgnoreCase);

    var body = await response.Content.ReadAsStringAsync();

    Assert.Contains("<title>Evidence Generator Documentation</title>", body, StringComparison.Ordinal);
    Assert.Contains("font-size: 16px", body, StringComparison.Ordinal);
    Assert.Contains("prefers-color-scheme: light", body, StringComparison.Ordinal);
    Assert.Contains("POST /generate", body, StringComparison.Ordinal);
    Assert.Contains("Stevan Freeborn", body, StringComparison.Ordinal);
    Assert.Contains("Generate with an LLM", body, StringComparison.Ordinal);
    Assert.Contains("copyPrompt()", body, StringComparison.Ordinal);
  }

  [Theory]
  [InlineData("/docs")]
  [InlineData("/help")]
  [InlineData("/readme")]
  [InlineData("/any/arbitrary/path")]
  public async Task Get_UnmatchedPaths_FallbackServesDocumentationHtml(string path)
  {
    var client = _factory.CreateClient();

    var response = await client.GetAsync(path);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.NotNull(response.Content.Headers.ContentType);
    Assert.Contains("text/html", response.Content.Headers.ContentType.MediaType, StringComparison.OrdinalIgnoreCase);

    var body = await response.Content.ReadAsStringAsync();

    Assert.Contains("<h1>Evidence Generator</h1>", body, StringComparison.Ordinal);
    Assert.Contains("Stevan Freeborn", body, StringComparison.Ordinal);
  }

  [Fact]
  public async Task Get_DocumentationPage_AllowsAnonymousAccessWithoutApiKey()
  {
    var client = _factory.CreateClient();

    var response = await client.GetAsync("/");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task Post_ToGenerate_ContinuesToRequireAuthentication()
  {
    var client = _factory.CreateClient();

    var response = await client.PostAsync("/generate", new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }
}