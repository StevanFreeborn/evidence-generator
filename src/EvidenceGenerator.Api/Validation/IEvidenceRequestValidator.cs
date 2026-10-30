using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Validation;

internal interface IEvidenceRequestValidator
{
  IDictionary<string, string[]> Validate(GenerateEvidenceRequest request);
}

internal interface IEvidenceTypeValidator
{
  SupportedFileType SupportedType { get; }
  IDictionary<string, string[]> Validate(GenerateEvidenceRequest request);
}

internal interface IEvidenceTypeValidator<in TRequest> : IEvidenceTypeValidator where TRequest : GenerateEvidenceRequest
{
  IDictionary<string, string[]> Validate(TRequest request);
}