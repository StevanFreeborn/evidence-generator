using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation.Validators;

internal sealed class XlsxEvidenceRequestValidator : EvidenceRequestValidatorBase<XlsxEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Xlsx;

  protected override void ValidateSubtype(XlsxEvidenceRequest request, ValidationResultBuilder builder)
  {
    if (request.Sheets is null || request.Sheets.Count is 0)
    {
      builder.AddError(nameof(request.Sheets), "Cannot be empty.");
      return;
    }

    var hasInvalidSheet = request.Sheets.Any(s =>
      string.IsNullOrWhiteSpace(s.SheetName) ||
      s.Columns is null ||
      s.Columns.Count is 0
    );

    if (hasInvalidSheet)
    {
      builder.AddError(nameof(request.Sheets), "Each sheet must have a non-empty SheetName and at least one column.");
    }
  }
}