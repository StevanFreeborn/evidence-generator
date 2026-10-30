using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation.Validators;

namespace EvidenceGenerator.Api.Tests.Validation;

public class PptxEvidenceRequestValidatorTests
{
  private readonly PptxEvidenceRequestValidator _validator = new();

  [Fact]
  public void Validate_EmptySlides_ReturnsError()
  {
    var request = new PptxEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.pptx",
      Slides = Array.Empty<PptxSlide>()
    };

    var errors = _validator.Validate(request);

    Assert.Contains(nameof(PptxEvidenceRequest.Slides), errors.Keys);
  }

  [Fact]
  public void Validate_ValidPptxRequest_ReturnsNoErrors()
  {
    var request = new PptxEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.pptx",
      Slides = [new PptxSlide("Slide 1", ["Point 1"])]
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }
}