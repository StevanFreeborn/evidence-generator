using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation;

namespace EvidenceGenerator.Api.Tests.Validation;

public class EvidenceRequestValidatorBaseTests
{
  private sealed record DummyEvidenceRequest : GenerateEvidenceRequest
  {
    public override SupportedFileType FileType => SupportedFileType.Txt;
  }

  private sealed class DummyValidator : EvidenceRequestValidatorBase<DummyEvidenceRequest>
  {
    public override SupportedFileType SupportedType => SupportedFileType.Txt;

    protected override void ValidateSubtype(DummyEvidenceRequest request, ValidationResultBuilder builder)
    {
    }
  }

  [Theory]
  [InlineData(0, 1, "test.txt")]
  [InlineData(-1, 1, "test.txt")]
  [InlineData(1, 0, "test.txt")]
  [InlineData(1, -1, "test.txt")]
  [InlineData(1, 1, "")]
  [InlineData(1, 1, "   ")]
  public void Validate_InvalidBaseProperties_ReturnsErrors(int appId, int attachmentFieldId, string fileName)
  {
    var validator = new DummyValidator();
    var request = new DummyEvidenceRequest
    {
      AppId = appId,
      AttachmentFieldId = attachmentFieldId,
      FileName = fileName
    };

    var errors = validator.Validate(request);

    Assert.NotEmpty(errors);

    if (appId <= 0)
    {
      Assert.True(errors.ContainsKey(nameof(GenerateEvidenceRequest.AppId)));
    }
    if (attachmentFieldId <= 0)
    {
      Assert.True(errors.ContainsKey(nameof(GenerateEvidenceRequest.AttachmentFieldId)));
    }
    if (string.IsNullOrWhiteSpace(fileName))
    {
      Assert.True(errors.ContainsKey(nameof(GenerateEvidenceRequest.FileName)));
    }
  }

  [Fact]
  public void Validate_ValidBaseProperties_ReturnsEmptyErrors()
  {
    var validator = new DummyValidator();
    var request = new DummyEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.txt"
    };

    var errors = validator.Validate(request);

    Assert.Empty(errors);
  }
}