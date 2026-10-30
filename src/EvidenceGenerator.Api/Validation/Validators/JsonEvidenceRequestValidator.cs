using System.Text.Json;

using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation.Validators;

internal sealed class JsonEvidenceRequestValidator : EvidenceRequestValidatorBase<JsonEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Json;

  protected override void ValidateSubtype(JsonEvidenceRequest request, ValidationResultBuilder builder)
  {
    var hasData = request.Data.HasValue && request.Data.Value.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null;
    var hasJson = string.IsNullOrWhiteSpace(request.Json) is false;

    if (hasData is false && hasJson is false)
    {
      builder.AddError(nameof(request.Data), $"Either '{nameof(JsonEvidenceRequest.Data)}' or '{nameof(JsonEvidenceRequest.Json)}' must be provided.");
      return;
    }

    if (hasJson)
    {
      try
      {
        using var _ = JsonDocument.Parse(request.Json!);
      }
      catch (JsonException ex)
      {
        builder.AddError(nameof(request.Json), $"Invalid JSON format: {ex.Message}");
      }
    }
  }
}