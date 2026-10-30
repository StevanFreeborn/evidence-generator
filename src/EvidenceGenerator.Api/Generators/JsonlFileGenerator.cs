using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Generators;

internal sealed class JsonlFileGenerator : FileGeneratorBase<JsonlEvidenceRequest>
{
  private static readonly JsonSerializerOptions CompactOptions = new()
  {
    WriteIndented = false,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
  };

  private static readonly byte[] NewlineBytes = "\n"u8.ToArray();

  public override SupportedFileType SupportedType => SupportedFileType.Jsonl;

  public override async Task<GeneratedFile> GenerateAsync(JsonlEvidenceRequest request, CancellationToken ct = default)
  {
    var stream = new MemoryStream();

    if (request.Lines is not null)
    {
      for (var i = 0; i < request.Lines.Count; i++)
      {
        await JsonSerializer.SerializeAsync(stream, request.Lines[i], CompactOptions, ct);
        await stream.WriteAsync(NewlineBytes.AsMemory(), ct);
      }
    }
    else if (string.IsNullOrWhiteSpace(request.Text) is false)
    {
      var rawLines = request.Text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

      foreach (var rawLine in rawLines)
      {
        var trimmed = rawLine.Trim();

        if (trimmed.Length == 0)
        {
          continue;
        }

        using var doc = JsonDocument.Parse(trimmed);
        await JsonSerializer.SerializeAsync(stream, doc.RootElement, CompactOptions, ct);
        await stream.WriteAsync(NewlineBytes.AsMemory(), ct);
      }
    }

    stream.Position = 0;

    return new GeneratedFile(stream, request.GetNormalizedFileName(), "application/x-ndjson");
  }
}