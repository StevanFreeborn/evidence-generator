namespace EvidenceGenerator.Api.Requests;

internal sealed record CsvEvidenceRequest : GenerateEvidenceRequest
{
  public override SupportedFileType FileType => SupportedFileType.Csv;
  public required IReadOnlyList<string> Headers { get; init; }
  public required IReadOnlyList<IReadOnlyList<string>> Rows { get; init; }
}