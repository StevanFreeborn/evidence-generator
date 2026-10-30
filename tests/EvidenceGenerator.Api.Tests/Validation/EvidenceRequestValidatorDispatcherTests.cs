using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation;

using NSubstitute;

namespace EvidenceGenerator.Api.Tests.Validation;

public class EvidenceRequestValidatorDispatcherTests
{
  [Theory]
  [InlineData(SupportedFileType.Txt)]
  [InlineData(SupportedFileType.Html)]
  [InlineData(SupportedFileType.Csv)]
  [InlineData(SupportedFileType.Xlsx)]
  [InlineData(SupportedFileType.Docx)]
  [InlineData(SupportedFileType.Pptx)]
  [InlineData(SupportedFileType.Pdf)]
  internal void Validate_DispatchesToMatchingValidator(SupportedFileType fileType)
  {
    var mockValidators = new List<IEvidenceTypeValidator>();
    IEvidenceTypeValidator? targetValidator = null;

    var expectedErrors = new Dictionary<string, string[]>
    {
      { "TestProp", new[] { "Test error" } }
    };

    foreach (var type in Enum.GetValues<SupportedFileType>())
    {
      var val = Substitute.For<IEvidenceTypeValidator>();
      val.SupportedType.Returns(type);
      val.Validate(Arg.Any<GenerateEvidenceRequest>()).Returns(expectedErrors);
      mockValidators.Add(val);

      if (type == fileType)
      {
        targetValidator = val;
      }
    }

    var dispatcher = new EvidenceRequestValidatorDispatcher(mockValidators);

    GenerateEvidenceRequest request = fileType switch
    {
      SupportedFileType.Txt => new TxtEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Text = "t" },
      SupportedFileType.Html => new HtmlEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Title = "t", MarkdownOrHtmlBody = "b" },
      SupportedFileType.Csv => new CsvEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Headers = ["h"], Rows = [] },
      SupportedFileType.Xlsx => new XlsxEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Sheets = [new XlsxSheet("s", ["c"], [])] },
      SupportedFileType.Docx => new DocxEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Title = "t", Sections = [new DocxSection("h", "p")] },
      SupportedFileType.Pptx => new PptxEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Slides = [new PptxSlide("s", ["b"])] },
      SupportedFileType.Pdf => new PdfEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Title = "t", Sections = [new PdfSection("h", "b")] },
      _ => throw new ArgumentException("Unsupported file type", nameof(fileType))
    };

    var result = dispatcher.Validate(request);

    Assert.Equal(expectedErrors, result);

    targetValidator!.Received(1).Validate(request);
  }

  [Fact]
  public void Validate_MissingValidator_ThrowsNotSupportedException()
  {
    var dispatcher = new EvidenceRequestValidatorDispatcher([]);
    var request = new TxtEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "f", Text = "t" };

    Assert.Throws<NotSupportedException>(() => dispatcher.Validate(request));
  }
}