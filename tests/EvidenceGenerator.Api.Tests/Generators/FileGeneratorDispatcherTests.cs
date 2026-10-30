using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

using NSubstitute;

namespace EvidenceGenerator.Api.Tests.Generators;

public class FileGeneratorDispatcherTests
{
  [Theory]
  [InlineData(SupportedFileType.Txt)]
  [InlineData(SupportedFileType.Html)]
  [InlineData(SupportedFileType.Csv)]
  [InlineData(SupportedFileType.Xlsx)]
  [InlineData(SupportedFileType.Docx)]
  [InlineData(SupportedFileType.Pptx)]
  [InlineData(SupportedFileType.Pdf)]
  [InlineData(SupportedFileType.Json)]
  [InlineData(SupportedFileType.Jsonl)]
  internal async Task Dispatch_RoutesToMatchingGenerator(SupportedFileType fileType)
  {
    var mockGenerators = new List<IFileGenerator>();
    IFileGenerator? targetGenerator = null;

    foreach (var type in Enum.GetValues<SupportedFileType>())
    {
      var gen = Substitute.For<IFileGenerator>();
      gen.SupportedType.Returns(type);

      var expectedFile = new GeneratedFile(new MemoryStream(), $"file.{type.ToString().ToLowerInvariant()}", "application/test");
      gen.GenerateAsync(Arg.Any<GenerateEvidenceRequest>(), Arg.Any<CancellationToken>())
        .Returns(Task.FromResult(expectedFile));

      mockGenerators.Add(gen);

      if (type == fileType)
      {
        targetGenerator = gen;
      }
    }

    var dispatcher = new FileGeneratorDispatcher(mockGenerators);

    GenerateEvidenceRequest request = fileType switch
    {
      SupportedFileType.Txt => new TxtEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Text = "t" },
      SupportedFileType.Html => new HtmlEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Title = "t", MarkdownOrHtmlBody = "b" },
      SupportedFileType.Csv => new CsvEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Headers = ["h"], Rows = [] },
      SupportedFileType.Xlsx => new XlsxEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Sheets = [new XlsxSheet("s", ["c"], [])] },
      SupportedFileType.Docx => new DocxEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Title = "t", Sections = [new DocxSection("h", "p")] },
      SupportedFileType.Pptx => new PptxEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Slides = [new PptxSlide("s", ["b"])] },
      SupportedFileType.Pdf => new PdfEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Title = "t", Sections = [new PdfSection("h", "b")] },
      SupportedFileType.Json => new JsonEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Json = "{}" },
      SupportedFileType.Jsonl => new JsonlEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Text = "{}\n" },
      _ => throw new ArgumentException("Unsupported file type", nameof(fileType))
    };

    var result = await dispatcher.GenerateAsync(request);

    Assert.NotNull(result);

    await targetGenerator!.Received(1).GenerateAsync(request, Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Dispatch_MissingGenerator_ThrowsNotSupportedException()
  {
    var dispatcher = new FileGeneratorDispatcher(Enumerable.Empty<IFileGenerator>());
    var request = new TxtEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Text = "t" };

    await Assert.ThrowsAsync<NotSupportedException>(() => dispatcher.GenerateAsync(request));
  }
}