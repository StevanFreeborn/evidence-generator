namespace EvidenceGenerator.Api.Requests;

internal sealed record HtmlEvidenceRequest : GenerateEvidenceRequest
{
  public override SupportedFileType FileType => SupportedFileType.Html;
  public required string Title { get; init; }
  public required string MarkdownOrHtmlBody { get; init; }
}