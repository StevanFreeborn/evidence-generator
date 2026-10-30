using System.Text.Json;

using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Tests.Generators;

public class JsonlFileGeneratorTests
{
  [Fact]
  public async Task GenerateAsync_WithLines_GeneratesValidJsonLines()
  {
    var generator = new JsonlFileGenerator();
    var doc1 = JsonDocument.Parse("""{"event":"login","userId":101}""");
    var doc2 = JsonDocument.Parse("""{"event":"logout","userId":101}""");

    var request = new JsonlEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "events",
      Lines = [doc1.RootElement, doc2.RootElement]
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("events.jsonl", result.FileName);
    Assert.Equal("application/x-ndjson", result.ContentType);

    using var reader = new StreamReader(result.Stream);
    var content = await reader.ReadToEndAsync();

    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    Assert.Equal(2, lines.Length);

    var parsed1 = JsonDocument.Parse(lines[0]);

    Assert.Equal("login", parsed1.RootElement.GetProperty("event").GetString());

    var parsed2 = JsonDocument.Parse(lines[1]);

    Assert.Equal("logout", parsed2.RootElement.GetProperty("event").GetString());
  }

  [Fact]
  public async Task GenerateAsync_WithText_GeneratesValidJsonLines()
  {
    var generator = new JsonlFileGenerator();
    var rawText = "{\"log\":\"first\"}\r\n{\"log\":\"second\"}\n";

    var request = new JsonlEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "logs.jsonl",
      Text = rawText
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("logs.jsonl", result.FileName);
    Assert.Equal("application/x-ndjson", result.ContentType);

    using var reader = new StreamReader(result.Stream);
    var content = await reader.ReadToEndAsync();

    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    Assert.Equal(2, lines.Length);

    var parsed1 = JsonDocument.Parse(lines[0]);

    Assert.Equal("first", parsed1.RootElement.GetProperty("log").GetString());

    var parsed2 = JsonDocument.Parse(lines[1]);

    Assert.Equal("second", parsed2.RootElement.GetProperty("log").GetString());
  }
}