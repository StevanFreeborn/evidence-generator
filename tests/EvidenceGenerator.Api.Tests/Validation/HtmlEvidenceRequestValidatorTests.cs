using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation.Validators;

namespace EvidenceGenerator.Api.Tests.Validation;

public class HtmlEvidenceRequestValidatorTests
{
  private readonly HtmlEvidenceRequestValidator _validator = new();

  [Theory]
  [InlineData("", "body")]
  [InlineData("   ", "body")]
  [InlineData("title", "")]
  [InlineData("title", "   ")]
  public void Validate_EmptyTitleOrBody_ReturnsError(string title, string body)
  {
    var request = new HtmlEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.html",
      Title = title,
      MarkdownOrHtmlBody = body
    };

    var errors = _validator.Validate(request);

    Assert.NotEmpty(errors);

    if (string.IsNullOrWhiteSpace(title))
    {
      Assert.Contains(nameof(HtmlEvidenceRequest.Title), errors.Keys);
    }

    if (string.IsNullOrWhiteSpace(body))
    {
      Assert.Contains(nameof(HtmlEvidenceRequest.MarkdownOrHtmlBody), errors.Keys);
    }
  }

  [Fact]
  public void Validate_ValidHtmlRequest_ReturnsNoErrors()
  {
    var request = new HtmlEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.html",
      Title = "Valid Title",
      MarkdownOrHtmlBody = "Valid Body"
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }
}