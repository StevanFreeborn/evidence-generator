using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation.Validators;

namespace EvidenceGenerator.Api.Tests.Validation;

public class PdfEvidenceRequestValidatorTests
{
  private readonly PdfEvidenceRequestValidator _validator = new();

  [Fact]
  public void Validate_EmptyTitleOrSections_ReturnsError()
  {
    var request = new PdfEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.pdf",
      Title = " ",
      Sections = Array.Empty<PdfSection>()
    };

    var errors = _validator.Validate(request);

    Assert.Contains(nameof(PdfEvidenceRequest.Title), errors.Keys);
    Assert.Contains(nameof(PdfEvidenceRequest.Sections), errors.Keys);
  }

  [Fact]
  public void Validate_ValidPdfRequest_ReturnsNoErrors()
  {
    var request = new PdfEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.pdf",
      Title = "Valid Title",
      Sections = [new PdfSection("Heading", "Body")]
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }
}