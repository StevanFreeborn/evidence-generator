using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Generators;

internal interface IFileGeneratorDispatcher
{
  Task<GeneratedFile> GenerateAsync(GenerateEvidenceRequest request, CancellationToken ct = default);
}