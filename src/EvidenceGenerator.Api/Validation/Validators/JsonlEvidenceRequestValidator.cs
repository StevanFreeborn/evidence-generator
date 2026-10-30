using System.Text.Json;

using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation.Validators;

internal sealed class JsonlEvidenceRequestValidator : EvidenceRequestValidatorBase<JsonlEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Jsonl;

  protected override void ValidateSubtype(JsonlEvidenceRequest request, ValidationResultBuilder builder)
  {
    var hasLines = request.Lines is not null;
    var hasText = string.IsNullOrWhiteSpace(request.Text) is false;

    if (hasLines is false && hasText is false)
    {
      builder.AddError(nameof(request.Lines), $"Either '{nameof(JsonlEvidenceRequest.Lines)}' or '{nameof(JsonlEvidenceRequest.Text)}' must be provided.");
      return;
    }

    if (hasLines)
    {
      if (request.Lines!.Count == 0)
      {
        builder.AddError(nameof(request.Lines), "Collection cannot be empty.");
      }
      else
      {
        for (var i = 0; i < request.Lines.Count; i++)
        {
          var line = request.Lines[i];

          if (line.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
          {
            builder.AddError($"{nameof(request.Lines)}[{i}]", "Cannot be null or undefined.");
          }
        }
      }
    }

    if (hasText)
    {
      var rawLines = request.Text!.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
      var nonBlankCount = 0;

      for (var i = 0; i < rawLines.Length; i++)
      {
        var line = rawLines[i].Trim();
        
        if (line.Length == 0)
        {
          continue;
        }

        nonBlankCount++;

        try
        {
          using var _ = JsonDocument.Parse(line);
        }
        catch (JsonException ex)
        {
          builder.AddError($"{nameof(request.Text)}[line {i + 1}]", $"Invalid JSON on line {i + 1}: {ex.Message}");
        }
      }

      if (nonBlankCount == 0)
      {
        builder.AddError(nameof(request.Text), "Must contain at least one valid JSON line.");
      }
    }
  }
}