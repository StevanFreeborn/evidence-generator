using System.Text;

using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Generators;

internal sealed class CsvFileGenerator : FileGeneratorBase<CsvEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Csv;

  public override async Task<GeneratedFile> GenerateAsync(CsvEvidenceRequest request, CancellationToken ct = default)
  {
    var stream = new MemoryStream();

    await using var writer = new StreamWriter(stream, new UTF8Encoding(false), bufferSize: 1024, leaveOpen: true);

    writer.NewLine = "\r\n";

    if (request.Headers.Count > 0)
    {
      await writer.WriteLineAsync(FormatRow(request.Headers));
    }

    if (request.Rows is not null)
    {
      foreach (var row in request.Rows)
      {
        ct.ThrowIfCancellationRequested();
        await writer.WriteLineAsync(FormatRow(row));
      }
    }

    await writer.FlushAsync(ct);

    stream.Position = 0;

    return new GeneratedFile(stream, request.GetNormalizedFileName(), "text/csv");
  }

  private static string FormatRow(IEnumerable<string?> fields)
  {
    return string.Join(",", fields.Select(EscapeField));
  }

  private static string EscapeField(string? field)
  {
    if (string.IsNullOrEmpty(field))
    {
      return string.Empty;
    }

    var mustQuote = field.Contains(',', StringComparison.Ordinal)
      || field.Contains('"', StringComparison.Ordinal)
      || field.Contains('\r', StringComparison.Ordinal)
      || field.Contains('\n', StringComparison.Ordinal);

    if (mustQuote is false)
    {
      return field;
    }

    var escaped = field.Replace("\"", "\"\"", StringComparison.Ordinal);

    return $"\"{escaped}\"";
  }
}