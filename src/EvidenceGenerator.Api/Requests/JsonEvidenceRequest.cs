using System.Text.Json;

namespace EvidenceGenerator.Api.Requests;

internal sealed record JsonEvidenceRequest : GenerateEvidenceRequest
{
  public override SupportedFileType FileType => SupportedFileType.Json;
  public JsonElement? Data { get; init; }
  public string? Json { get; init; }
}
