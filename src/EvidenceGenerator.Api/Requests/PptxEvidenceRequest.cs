namespace EvidenceGenerator.Api.Requests;

internal sealed record PptxSlide(string Title, IReadOnlyList<string> BulletPoints);

internal sealed record PptxEvidenceRequest : GenerateEvidenceRequest
{
  public override SupportedFileType FileType => SupportedFileType.Pptx;
  public required IReadOnlyList<PptxSlide> Slides { get; init; }
}