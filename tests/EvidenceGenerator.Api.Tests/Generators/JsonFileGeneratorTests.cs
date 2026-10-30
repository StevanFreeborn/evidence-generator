using System.Text.Json;

using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Tests.Generators;

public class JsonFileGeneratorTests
{
  [Fact]
  public async Task GenerateAsync_WithData_GeneratesIndentedJson()
  {
    var generator = new JsonFileGenerator();
    var sampleDoc = JsonDocument.Parse("""{"key":"value","count":42}""");
    var request = new JsonEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "evidence",
      Data = sampleDoc.RootElement
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("evidence.json", result.FileName);
    Assert.Equal("application/json", result.ContentType);

    using var reader = new StreamReader(result.Stream);
    var content = await reader.ReadToEndAsync();

    var parsed = JsonDocument.Parse(content);

    Assert.Equal("value", parsed.RootElement.GetProperty("key").GetString());
    Assert.Equal(42, parsed.RootElement.GetProperty("count").GetInt32());
    Assert.Contains("\n", content);
  }

  [Fact]
  public async Task GenerateAsync_WithRawJson_GeneratesIndentedJson()
  {
    var generator = new JsonFileGenerator();
    var request = new JsonEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "audit.json",
      Json = """{"service":"iam","status":"healthy"}"""
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("audit.json", result.FileName);
    Assert.Equal("application/json", result.ContentType);

    using var reader = new StreamReader(result.Stream);
    var content = await reader.ReadToEndAsync();

    var parsed = JsonDocument.Parse(content);

    Assert.Equal("iam", parsed.RootElement.GetProperty("service").GetString());
    Assert.Equal("healthy", parsed.RootElement.GetProperty("status").GetString());
    Assert.Contains("\n", content);
  }
}