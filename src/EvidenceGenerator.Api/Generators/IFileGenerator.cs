using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Generators;

internal interface IFileGenerator
{
  SupportedFileType SupportedType { get; }
  Task<GeneratedFile> GenerateAsync(GenerateEvidenceRequest request, CancellationToken ct = default);
}

internal interface IFileGenerator<in TRequest> : IFileGenerator where TRequest : GenerateEvidenceRequest
{
  Task<GeneratedFile> GenerateAsync(TRequest request, CancellationToken ct = default);
}