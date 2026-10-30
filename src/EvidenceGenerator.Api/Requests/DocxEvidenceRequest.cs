namespace EvidenceGenerator.Api.Requests;

internal sealed record DocxSection(string? Heading, string ParagraphText);

internal sealed record DocxTable(IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows);

internal sealed record DocxEvidenceRequest : GenerateEvidenceRequest
{
  public override SupportedFileType FileType => SupportedFileType.Docx;
  public required string Title { get; init; }
  public required IReadOnlyList<DocxSection> Sections { get; init; }
  public DocxTable? Table { get; init; }
}