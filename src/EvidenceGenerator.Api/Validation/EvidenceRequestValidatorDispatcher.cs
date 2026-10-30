using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation;

internal sealed class EvidenceRequestValidatorDispatcher(IEnumerable<IEvidenceTypeValidator> validators) : IEvidenceRequestValidator
{
  private readonly Dictionary<SupportedFileType, IEvidenceTypeValidator> _validators = validators.ToDictionary(v => v.SupportedType);

  public IDictionary<string, string[]> Validate(GenerateEvidenceRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);

    if (_validators.TryGetValue(request.FileType, out var validator) is false)
    {
      throw new NotSupportedException($"No validator registered for file type '{request.FileType}'.");
    }

    return validator.Validate(request);
  }
}