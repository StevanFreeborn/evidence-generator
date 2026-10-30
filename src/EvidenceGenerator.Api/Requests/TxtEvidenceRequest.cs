namespace EvidenceGenerator.Api.Requests;

internal sealed record TxtEvidenceRequest : GenerateEvidenceRequest
{
  public override SupportedFileType FileType => SupportedFileType.Txt;
  public required string Text { get; init; }
}