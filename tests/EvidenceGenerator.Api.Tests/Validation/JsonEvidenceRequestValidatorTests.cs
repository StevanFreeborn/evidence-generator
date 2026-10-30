using System.Text.Json;

using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation.Validators;

namespace EvidenceGenerator.Api.Tests.Validation;

public class JsonEvidenceRequestValidatorTests
{
  private readonly JsonEvidenceRequestValidator _validator = new();

  [Fact]
  public void Validate_MissingDataAndJson_ReturnsError()
  {
    var request = new JsonEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.json"
    };

    var errors = _validator.Validate(request);

    Assert.Contains("Data", errors.Keys);
  }

  [Fact]
  public void Validate_InvalidJsonString_ReturnsError()
  {
    var request = new JsonEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.json",
      Json = "{ not valid json "
    };

    var errors = _validator.Validate(request);

    Assert.Contains("Json", errors.Keys);
  }

  [Fact]
  public void Validate_ValidJsonString_ReturnsNoErrors()
  {
    var request = new JsonEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.json",
      Json = "{\"valid\": true}"
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }

  [Fact]
  public void Validate_ValidDataElement_ReturnsNoErrors()
  {
    using var doc = JsonDocument.Parse("{\"item\": 123}");
    var request = new JsonEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.json",
      Data = doc.RootElement
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }
}
