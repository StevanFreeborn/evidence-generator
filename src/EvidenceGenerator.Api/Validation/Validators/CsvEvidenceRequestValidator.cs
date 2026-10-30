using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation.Validators;

internal sealed class CsvEvidenceRequestValidator : EvidenceRequestValidatorBase<CsvEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Csv;

  protected override void ValidateSubtype(CsvEvidenceRequest request, ValidationResultBuilder builder)
  {
    if (request.Headers is null || request.Headers.Count is 0)
    {
      builder.AddError(nameof(request.Headers), "Cannot be empty.");
    }
  }
}