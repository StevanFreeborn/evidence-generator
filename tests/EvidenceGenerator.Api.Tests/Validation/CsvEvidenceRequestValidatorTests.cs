using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation.Validators;

namespace EvidenceGenerator.Api.Tests.Validation;

public class CsvEvidenceRequestValidatorTests
{
  private readonly CsvEvidenceRequestValidator _validator = new();

  [Fact]
  public void Validate_EmptyHeaders_ReturnsError()
  {
    var request = new CsvEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.csv",
      Headers = [],
      Rows = []
    };

    var errors = _validator.Validate(request);

    Assert.Contains(nameof(CsvEvidenceRequest.Headers), errors.Keys);
  }

  [Fact]
  public void Validate_ValidCsvRequest_ReturnsNoErrors()
  {
    var request = new CsvEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.csv",
      Headers = ["Header1"],
      Rows = [["Value1"]]
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }
}