namespace EvidenceGenerator.Api.Requests;

internal sealed record PdfSection(string? Heading, string Body);

internal sealed record PdfEvidenceRequest : GenerateEvidenceRequest
{
  public override SupportedFileType FileType => SupportedFileType.Pdf;
  public required string Title { get; init; }
  public required IReadOnlyList<PdfSection> Sections { get; init; }
}