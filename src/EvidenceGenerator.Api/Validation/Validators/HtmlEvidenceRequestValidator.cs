using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation.Validators;

internal sealed class HtmlEvidenceRequestValidator : EvidenceRequestValidatorBase<HtmlEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Html;

  protected override void ValidateSubtype(HtmlEvidenceRequest request, ValidationResultBuilder builder)
  {
    if (string.IsNullOrWhiteSpace(request.Title))
    {
      builder.AddError(nameof(request.Title), "Cannot be empty.");
    }

    if (string.IsNullOrWhiteSpace(request.MarkdownOrHtmlBody))
    {
      builder.AddError(nameof(request.MarkdownOrHtmlBody), "Cannot be empty.");
    }
  }
}