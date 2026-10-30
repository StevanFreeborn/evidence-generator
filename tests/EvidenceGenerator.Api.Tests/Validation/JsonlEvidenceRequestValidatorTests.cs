using System.Text.Json;

using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation.Validators;

namespace EvidenceGenerator.Api.Tests.Validation;

public class JsonlEvidenceRequestValidatorTests
{
  private readonly JsonlEvidenceRequestValidator _validator = new();

  [Fact]
  public void Validate_MissingLinesAndText_ReturnsError()
  {
    var request = new JsonlEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.jsonl"
    };

    var errors = _validator.Validate(request);

    Assert.Contains("Lines", errors.Keys);
  }

  [Fact]
  public void Validate_EmptyLinesList_ReturnsError()
  {
    var request = new JsonlEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.jsonl",
      Lines = []
    };

    var errors = _validator.Validate(request);

    Assert.Contains("Lines", errors.Keys);
  }

  [Fact]
  public void Validate_InvalidTextLine_ReturnsError()
  {
    var request = new JsonlEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.jsonl",
      Text = "{\"valid\":1}\n{not json}"
    };

    var errors = _validator.Validate(request);

    Assert.Contains("Text[line 2]", errors.Keys);
  }

  [Fact]
  public void Validate_ValidLines_ReturnsNoErrors()
  {
    using var doc = JsonDocument.Parse("{\"id\":1}");
    var request = new JsonlEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.jsonl",
      Lines = [doc.RootElement]
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }

  [Fact]
  public void Validate_ValidText_ReturnsNoErrors()
  {
    var request = new JsonlEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.jsonl",
      Text = "{\"a\":1}\n{\"b\":2}"
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }
}
