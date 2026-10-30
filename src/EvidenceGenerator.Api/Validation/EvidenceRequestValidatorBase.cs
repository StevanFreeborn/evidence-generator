using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation;

internal abstract class EvidenceRequestValidatorBase<TRequest> : IEvidenceTypeValidator<TRequest> where TRequest : GenerateEvidenceRequest
{
  public abstract SupportedFileType SupportedType { get; }

  public IDictionary<string, string[]> Validate(TRequest request)
  {
    var builder = new ValidationResultBuilder();

    if (request.AppId <= 0)
    {
      builder.AddError(nameof(request.AppId), "Must be greater than zero.");
    }

    if (request.AttachmentFieldId <= 0)
    {
      builder.AddError(nameof(request.AttachmentFieldId), "Must be greater than zero.");
    }

    if (string.IsNullOrWhiteSpace(request.FileName))
    {
      builder.AddError(nameof(request.FileName), "Cannot be empty.");
    }

    ValidateSubtype(request, builder);

    return builder.ToDictionary();
  }

  protected abstract void ValidateSubtype(TRequest request, ValidationResultBuilder builder);

  IDictionary<string, string[]> IEvidenceTypeValidator.Validate(GenerateEvidenceRequest request)
  {
    if (request is not TRequest typedRequest)
    {
      throw new ArgumentException($"Expected request of type {typeof(TRequest).Name}, but received {request?.GetType().Name}.", nameof(request));
    }

    return Validate(typedRequest);
  }
}