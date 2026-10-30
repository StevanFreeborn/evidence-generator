using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation.Validators;

internal sealed class TxtEvidenceRequestValidator : EvidenceRequestValidatorBase<TxtEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Txt;

  protected override void ValidateSubtype(TxtEvidenceRequest request, ValidationResultBuilder builder)
  {
    if (string.IsNullOrWhiteSpace(request.Text))
    {
      builder.AddError(nameof(request.Text), "Cannot be empty.");
    }
  }
}