using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation.Validators;

internal sealed class PptxEvidenceRequestValidator : EvidenceRequestValidatorBase<PptxEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Pptx;

  protected override void ValidateSubtype(PptxEvidenceRequest request, ValidationResultBuilder builder)
  {
    if (request.Slides is null || request.Slides.Count is 0)
    {
      builder.AddError(nameof(request.Slides), "Cannot be empty.");
    }
  }
}