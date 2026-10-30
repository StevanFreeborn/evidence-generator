using System.Net;
using System.Text;
using System.Text.Json;

using EvidenceGenerator.Api.Auth;
using EvidenceGenerator.Api.Background;
using EvidenceGenerator.Api.Requests;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace EvidenceGenerator.Api.Tests.Endpoints;

public class GenerateEndpointTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
  private readonly CustomWebApplicationFactory _factory = factory;

  [Fact]
  public async Task PostGenerate_MissingOrWhitespaceApiKey_Returns401UnauthorizedEvenWithInvalidBody()
  {
    var client = _factory.CreateClient();

    var content = new StringContent("{ invalid json", Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/generate", content);

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

    var request = new HttpRequestMessage(HttpMethod.Post, "/generate")
    {
      Content = new StringContent("{}", Encoding.UTF8, "application/json")
    };

    request.Headers.Add(OnspringApiKeyAuthenticationDefaults.HeaderName, "   ");
    var response2 = await client.SendAsync(request);

    Assert.Equal(HttpStatusCode.Unauthorized, response2.StatusCode);
  }

  [Fact]
  public async Task PostGenerate_InvalidDiscriminatorOrMalformedJson_Returns400BadRequest()
  {
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Add(OnspringApiKeyAuthenticationDefaults.HeaderName, "test-api-key");

    var invalidTypePayload = """{"fileType":"unsupported","appId":1,"attachmentFieldId":2,"fileName":"test.txt"}""";
    var content = new StringContent(invalidTypePayload, Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/generate", content);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var malformedContent = new StringContent("{ malformed", Encoding.UTF8, "application/json");
    var response2 = await client.PostAsync("/generate", malformedContent);

    Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
  }

  [Fact]
  public async Task PostGenerate_ValidationFailure_Returns400BadRequestValidationProblemDetails()
  {
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Add(OnspringApiKeyAuthenticationDefaults.HeaderName, "test-api-key");

    var invalidPayload = """
      {
        "fileType": "txt",
        "appId": 0,
        "attachmentFieldId": 2,
        "fileName": "test.txt",
        "text": "sample"
      }
    """;
    var content = new StringContent(invalidPayload, Encoding.UTF8, "application/json");

    var response = await client.PostAsync("/generate", content);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    var body = await response.Content.ReadAsStringAsync();

    Assert.Contains("AppId", body);
  }

  [Fact]
  public async Task PostGenerate_ValidPolymorphicRequest_Returns202AcceptedAndEnqueuesJob()
  {
    var mockQueue = Substitute.For<IBackgroundJobQueue>();
    mockQueue.DequeueAsync(Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var ct = callInfo.Arg<CancellationToken>();
        await Task.Delay(Timeout.Infinite, ct);
        return null!;
      });

    using var factory = _factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureServices(services =>
      {
        services.AddSingleton(mockQueue);
      });
    });

    var client = factory.CreateClient();

    client.DefaultRequestHeaders.Add(OnspringApiKeyAuthenticationDefaults.HeaderName, "test-api-key");

    var payload = """
      {
        "fileType": "txt",
        "appId": 10,
        "attachmentFieldId": 20,
        "fileName": "audit",
        "text": "Audit completed."
      }
    """;

    var content = new StringContent(payload, Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/generate", content);

    Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

    var responseJson = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(responseJson);
    var root = doc.RootElement;

    Assert.True(root.TryGetProperty("jobId", out var jobIdProp));
    Assert.True(Guid.TryParse(jobIdProp.GetString(), out var jobId));
    Assert.NotEqual(Guid.Empty, jobId);

    Assert.True(root.TryGetProperty("status", out var statusProp));
    Assert.Equal("Queued", statusProp.GetString());

    await mockQueue.Received(1).EnqueueAsync(
      Arg.Is<EvidenceJob>(j =>
        j.JobId == jobId &&
        j.ApiKey == "test-api-key" &&
        j.Request is TxtEvidenceRequest &&
        j.Request.AppId == 10
      ),
      Arg.Any<CancellationToken>()
    );
  }

  [Fact]
  public async Task PostGenerate_ValidJsonRequest_Returns202AcceptedAndEnqueuesJob()
  {
    var mockQueue = Substitute.For<IBackgroundJobQueue>();
    mockQueue.DequeueAsync(Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var ct = callInfo.Arg<CancellationToken>();
        await Task.Delay(Timeout.Infinite, ct);
        return null!;
      });

    using var factory = _factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureServices(services =>
      {
        services.AddSingleton(mockQueue);
      });
    });

    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Add(OnspringApiKeyAuthenticationDefaults.HeaderName, "test-api-key");

    var payload = """
      {
        "fileType": "json",
        "appId": 15,
        "attachmentFieldId": 25,
        "fileName": "security_audit",
        "data": { "service": "iam", "compliant": true }
      }
    """;

    var content = new StringContent(payload, Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/generate", content);

    Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

    await mockQueue.Received(1).EnqueueAsync(
      Arg.Is<EvidenceJob>(j =>
        j.ApiKey == "test-api-key" &&
        j.Request is JsonEvidenceRequest &&
        j.Request.AppId == 15
      ),
      Arg.Any<CancellationToken>()
    );
  }

  [Fact]
  public async Task PostGenerate_ValidJsonlRequest_Returns202AcceptedAndEnqueuesJob()
  {
    var mockQueue = Substitute.For<IBackgroundJobQueue>();
    mockQueue.DequeueAsync(Arg.Any<CancellationToken>())
      .Returns(async callInfo =>
      {
        var ct = callInfo.Arg<CancellationToken>();
        await Task.Delay(Timeout.Infinite, ct);
        return null!;
      });

    using var factory = _factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureServices(services =>
      {
        services.AddSingleton(mockQueue);
      });
    });

    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Add(OnspringApiKeyAuthenticationDefaults.HeaderName, "test-api-key");

    var payload = """
      {
        "fileType": "jsonl",
        "appId": 18,
        "attachmentFieldId": 28,
        "fileName": "cloudtrail_events",
        "lines": [
          { "eventId": "1", "action": "AssumeRole" },
          { "eventId": "2", "action": "ListBuckets" }
        ]
      }
    """;

    var content = new StringContent(payload, Encoding.UTF8, "application/json");
    var response = await client.PostAsync("/generate", content);

    Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

    await mockQueue.Received(1).EnqueueAsync(
      Arg.Is<EvidenceJob>(j =>
        j.ApiKey == "test-api-key" &&
        j.Request is JsonlEvidenceRequest &&
        j.Request.AppId == 18
      ),
      Arg.Any<CancellationToken>()
    );
  }
}