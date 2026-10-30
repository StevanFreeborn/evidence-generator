using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation.Validators;

namespace EvidenceGenerator.Api.Tests.Validation;

public class TxtEvidenceRequestValidatorTests
{
  private readonly TxtEvidenceRequestValidator _validator = new();

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Validate_EmptyOrWhitespaceText_ReturnsError(string text)
  {
    var request = new TxtEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.txt",
      Text = text
    };

    var errors = _validator.Validate(request);

    Assert.Contains(nameof(TxtEvidenceRequest.Text), errors.Keys);
  }

  [Fact]
  public void Validate_ValidText_ReturnsNoErrors()
  {
    var request = new TxtEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.txt",
      Text = "Valid content"
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }
}