using System.Diagnostics.CodeAnalysis;

namespace EvidenceGenerator.Api.Requests;

internal abstract record GenerateEvidenceRequest
{
  public required int AppId { get; init; }
  public required int AttachmentFieldId { get; init; }
  public required string FileName { get; init; }
  public string? Notes { get; init; }
  public Dictionary<int, object?>? RecordFields { get; init; }
  public abstract SupportedFileType FileType { get; }

  [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "File extensions are conventionally lowercase.")]
  public string GetNormalizedFileName()
  {
    var ext = $".{FileType.ToString().ToLowerInvariant()}";
    var trimmed = (FileName ?? string.Empty).Trim();
    return trimmed.EndsWith(ext, StringComparison.OrdinalIgnoreCase)
        ? trimmed
        : $"{trimmed}{ext}";
  }
}