using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation.Validators;

namespace EvidenceGenerator.Api.Tests.Validation;

public class XlsxEvidenceRequestValidatorTests
{
  private readonly XlsxEvidenceRequestValidator _validator = new();

  [Fact]
  public void Validate_EmptySheets_ReturnsError()
  {
    var request = new XlsxEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.xlsx",
      Sheets = []
    };

    var errors = _validator.Validate(request);

    Assert.Contains(nameof(XlsxEvidenceRequest.Sheets), errors.Keys);
  }

  [Fact]
  public void Validate_InvalidSheetWithEmptyNameOrColumns_ReturnsError()
  {
    var request = new XlsxEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.xlsx",
      Sheets = [
        new XlsxSheet("", [], [])
      ]
    };

    var errors = _validator.Validate(request);

    Assert.Contains(nameof(XlsxEvidenceRequest.Sheets), errors.Keys);
  }

  [Fact]
  public void Validate_ValidXlsxRequest_ReturnsNoErrors()
  {
    var request = new XlsxEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.xlsx",
      Sheets = [
        new XlsxSheet("Sheet1", ["Col1"], [["Val1"]])
      ]
    };

    var errors = _validator.Validate(request);

    Assert.Empty(errors);
  }
}