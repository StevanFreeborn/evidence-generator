using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation.Validators;

namespace EvidenceGenerator.Api.Tests.Validation;

public class DocxEvidenceRequestValidatorTests
{
  private readonly DocxEvidenceRequestValidator _validator = new();

  [Fact]
  public void Validate_EmptyTitleOrSections_ReturnsError()
  {
    var request = new DocxEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.docx",
      Title = "",
      Sections = []
    };

    var errors = _validator.Validate(request);

    Assert.Contains(nameof(DocxEvidenceRequest.Title), errors.Keys);
    Assert.Contains(nameof(DocxEvidenceRequest.Sections), errors.Keys);
  }

  [Fact]
  public void Validate_ValidDocxRequest_ReturnsNoErrors()
  {
    var request = new DocxEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.docx",
      Title = "Valid Title",
      Sections = [new DocxSection("Heading", "Paragraph")]
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }
}