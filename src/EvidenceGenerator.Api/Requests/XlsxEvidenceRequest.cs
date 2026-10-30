namespace EvidenceGenerator.Api.Requests;

internal sealed record XlsxSheet(
  string SheetName,
  IReadOnlyList<string> Columns,
  IReadOnlyList<IReadOnlyList<string>> Rows
);

internal sealed record XlsxEvidenceRequest : GenerateEvidenceRequest
{
  public override SupportedFileType FileType => SupportedFileType.Xlsx;
  public required IReadOnlyList<XlsxSheet> Sheets { get; init; }
}