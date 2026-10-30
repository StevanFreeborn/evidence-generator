using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation.Validators;

internal sealed class PdfEvidenceRequestValidator : EvidenceRequestValidatorBase<PdfEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Pdf;

  protected override void ValidateSubtype(PdfEvidenceRequest request, ValidationResultBuilder builder)
  {
    if (string.IsNullOrWhiteSpace(request.Title))
    {
      builder.AddError(nameof(request.Title), "Cannot be empty.");
    }

    if (request.Sections is null || request.Sections.Count is 0)
    {
      builder.AddError(nameof(request.Sections), "Cannot be empty.");
    }
  }
}