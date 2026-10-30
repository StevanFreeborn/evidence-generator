using ClosedXML.Excel;

using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Generators;

internal sealed class XlsxFileGenerator : FileGeneratorBase<XlsxEvidenceRequest>
{
  public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

  public override SupportedFileType SupportedType => SupportedFileType.Xlsx;

  public override Task<GeneratedFile> GenerateAsync(XlsxEvidenceRequest request, CancellationToken ct = default)
  {
    using var workbook = new XLWorkbook();

    foreach (var sheet in request.Sheets)
    {
      ct.ThrowIfCancellationRequested();

      var ws = workbook.Worksheets.Add(sheet.SheetName);

      for (var c = 0; c < sheet.Columns.Count; c++)
      {
        ws.Cell(1, c + 1).Value = sheet.Columns[c];
      }

      if (sheet.Rows is not null)
      {
        for (var r = 0; r < sheet.Rows.Count; r++)
        {
          ct.ThrowIfCancellationRequested();

          var rowData = sheet.Rows[r];

          for (var c = 0; c < rowData.Count; c++)
          {
            ws.Cell(r + 2, c + 1).Value = rowData[c];
          }
        }
      }
    }

    var stream = new MemoryStream();
    workbook.SaveAs(stream);

    stream.Position = 0;

    return Task.FromResult(new GeneratedFile(stream, request.GetNormalizedFileName(), XlsxContentType));
  }
}