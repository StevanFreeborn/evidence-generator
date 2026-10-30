using System.Text.Json;

namespace EvidenceGenerator.Api.Requests;

internal sealed record JsonlEvidenceRequest : GenerateEvidenceRequest
{
  public override SupportedFileType FileType => SupportedFileType.Jsonl;
  public List<JsonElement>? Lines { get; init; }
  public string? Text { get; init; }
}
