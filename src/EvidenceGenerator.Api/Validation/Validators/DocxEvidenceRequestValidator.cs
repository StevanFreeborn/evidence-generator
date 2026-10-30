using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation.Validators;

internal sealed class DocxEvidenceRequestValidator : EvidenceRequestValidatorBase<DocxEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Docx;

  protected override void ValidateSubtype(DocxEvidenceRequest request, ValidationResultBuilder builder)
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