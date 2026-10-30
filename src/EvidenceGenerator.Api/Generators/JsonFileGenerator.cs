using System.Text.Encodings.Web;
using System.Text.Json;

using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Generators;

internal sealed class JsonFileGenerator : FileGeneratorBase<JsonEvidenceRequest>
{
  private static readonly JsonSerializerOptions IndentedOptions = new()
  {
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
  };

  public override SupportedFileType SupportedType => SupportedFileType.Json;

  public override async Task<GeneratedFile> GenerateAsync(JsonEvidenceRequest request, CancellationToken ct = default)
  {
    var stream = new MemoryStream();

    if (request.Data.HasValue && request.Data.Value.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null)
    {
      await JsonSerializer.SerializeAsync(stream, request.Data.Value, IndentedOptions, ct);
    }
    else if (string.IsNullOrWhiteSpace(request.Json) is false)
    {
      using var doc = JsonDocument.Parse(request.Json);
      await JsonSerializer.SerializeAsync(stream, doc.RootElement, IndentedOptions, ct);
    }
    else
    {
      await JsonSerializer.SerializeAsync(stream, new { }, IndentedOptions, ct);
    }

    stream.Position = 0;

    return new GeneratedFile(stream, request.GetNormalizedFileName(), "application/json");
  }
}