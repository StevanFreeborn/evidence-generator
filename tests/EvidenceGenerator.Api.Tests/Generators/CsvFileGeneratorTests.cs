using System.Text;

using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Tests.Generators;

public class CsvFileGeneratorTests
{
  [Fact]
  public async Task GenerateAsync_HeadersAndRowsWithSpecialChars_ProperlyEscapesRfc4180()
  {
    var generator = new CsvFileGenerator();
    var request = new CsvEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "data",
      Headers = ["Name", "Desc, Special", "Quote\"Field", "Multi\nLine"],
      Rows = [
        ["Simple", "Has, comma", "Has \"quotes\"", "Line1\r\nLine2"],
        ["Normal", "Plain", "Standard", "Single"]
      ]
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("data.csv", result.FileName);
    Assert.Equal("text/csv", result.ContentType);
    Assert.Equal(0, result.Stream.Position);

    using var reader = new StreamReader(result.Stream, Encoding.UTF8, leaveOpen: true);
    var csvContent = await reader.ReadToEndAsync();

    var expectedLines = new[]
    {
      "Name,\"Desc, Special\",\"Quote\"\"Field\",\"Multi\nLine\"",
      "Simple,\"Has, comma\",\"Has \"\"quotes\"\"\",\"Line1\r\nLine2\"",
      "Normal,Plain,Standard,Single"
    };

    var expectedCsv = string.Join("\r\n", expectedLines) + "\r\n";

    Assert.Equal(expectedCsv, csvContent);
  }
}