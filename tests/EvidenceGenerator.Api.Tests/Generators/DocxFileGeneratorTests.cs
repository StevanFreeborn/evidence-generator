using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Tests.Generators;

public class DocxFileGeneratorTests
{
  [Fact]
  public async Task GenerateAsync_TitleSectionsAndTable_CreatesValidWordDocument()
  {
    var generator = new DocxFileGenerator();
    var request = new DocxEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "document",
      Title = "Annual Audit Report",
      Sections = [
        new DocxSection(Heading: "Executive Summary", ParagraphText: "Everything is verified."),
        new DocxSection(Heading: null, ParagraphText: "Additional notes without heading.")
      ],
      Table = new DocxTable(
        Headers: ["Item", "Status"],
        Rows: [
          ["Control 1", "Compliant"],
          ["Control 2", "In Review"]
        ]
      )
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("document.docx", result.FileName);
    Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", result.ContentType);
    Assert.Equal(0, result.Stream.Position);

    using var doc = WordprocessingDocument.Open(result.Stream, false);

    Assert.NotNull(doc.MainDocumentPart);
    Assert.NotNull(doc.MainDocumentPart.Document);
    Assert.NotNull(doc.MainDocumentPart.Document.Body);

    var body = doc.MainDocumentPart.Document.Body;
    var fullText = body.InnerText;

    Assert.Contains("Annual Audit Report", fullText);
    Assert.Contains("Executive Summary", fullText);
    Assert.Contains("Everything is verified.", fullText);
    Assert.Contains("Additional notes without heading.", fullText);
    Assert.Contains("Control 1", fullText);
    Assert.Contains("Compliant", fullText);
    Assert.Contains("Control 2", fullText);
    Assert.Contains("In Review", fullText);

    var tables = body.Elements<Table>().ToList();

    Assert.Single(tables);
  }
}