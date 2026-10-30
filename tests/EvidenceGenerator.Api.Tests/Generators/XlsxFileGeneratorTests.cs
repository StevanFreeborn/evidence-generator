using ClosedXML.Excel;

using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Tests.Generators;

public class XlsxFileGeneratorTests
{
  [Fact]
  public async Task GenerateAsync_MultipleSheets_CreatesValidXlsxWorkbook()
  {
    var generator = new XlsxFileGenerator();
    var request = new XlsxEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "spreadsheet",
      Sheets = [
        new XlsxSheet(
          SheetName: "Summary",
          Columns: ["Metric", "Value"],
          Rows: [
            ["Total", "100"],
            ["Passed", "95"]
          ]
        ),
        new XlsxSheet(
          SheetName: "Details",
          Columns: ["Id", "Description", "Status"],
          Rows: [
            ["1", "Check A", "OK"],
            ["2", "Check B", "Flagged"]
          ]
        )
      ]
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("spreadsheet.xlsx", result.FileName);
    Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
    Assert.Equal(0, result.Stream.Position);

    using var workbook = new XLWorkbook(result.Stream);

    Assert.Equal(2, workbook.Worksheets.Count);

    var sheet1 = workbook.Worksheet("Summary");

    Assert.NotNull(sheet1);
    Assert.Equal("Metric", sheet1.Cell(1, 1).GetString());
    Assert.Equal("Value", sheet1.Cell(1, 2).GetString());
    Assert.Equal("Total", sheet1.Cell(2, 1).GetString());
    Assert.Equal("100", sheet1.Cell(2, 2).GetString());
    Assert.Equal("Passed", sheet1.Cell(3, 1).GetString());
    Assert.Equal("95", sheet1.Cell(3, 2).GetString());

    var sheet2 = workbook.Worksheet("Details");

    Assert.NotNull(sheet2);
    Assert.Equal("Id", sheet2.Cell(1, 1).GetString());
    Assert.Equal("Description", sheet2.Cell(1, 2).GetString());
    Assert.Equal("Status", sheet2.Cell(1, 3).GetString());
    Assert.Equal("1", sheet2.Cell(2, 1).GetString());
    Assert.Equal("Check A", sheet2.Cell(2, 2).GetString());
    Assert.Equal("OK", sheet2.Cell(2, 3).GetString());
  }
}