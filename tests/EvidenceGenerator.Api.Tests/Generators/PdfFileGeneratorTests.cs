using System.Text;

using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

using PdfSharpCore.Pdf.IO;

namespace EvidenceGenerator.Api.Tests.Generators;

public class PdfFileGeneratorTests
{
  [Fact]
  public async Task GenerateAsync_SectionsWithHeadingsAndBodies_CreatesValidPdfDocument()
  {
    PdfFileGenerator.EnsureFontResolver();

    var generator = new PdfFileGenerator();
    var request = new PdfEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "compliance_report",
      Title = "SOC 2 Type II Compliance Report",
      Sections = [
        new PdfSection("Executive Summary", "This is the summary of the audit findings."),
        new PdfSection("Security Controls", "Access control policies are rigorously enforced.")
      ]
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("compliance_report.pdf", result.FileName);
    Assert.Equal("application/pdf", result.ContentType);
    Assert.Equal(0, result.Stream.Position);

    var buffer = new byte[5];
    var bytesRead = await result.Stream.ReadAsync(buffer.AsMemory(0, 5));

    Assert.Equal(5, bytesRead);

    var header = Encoding.ASCII.GetString(buffer);

    Assert.Equal("%PDF-", header);

    result.Stream.Position = 0;

    var doc = PdfReader.Open(result.Stream, PdfDocumentOpenMode.ReadOnly);

    Assert.True(doc.PageCount >= 1);
  }
}