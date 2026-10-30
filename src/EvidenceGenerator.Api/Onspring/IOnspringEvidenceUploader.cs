using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Onspring;

internal interface IOnspringEvidenceUploader
{
  Task UploadAsync(
    string apiKey,
    GenerateEvidenceRequest request,
    GeneratedFile generatedFile,
    CancellationToken ct = default
  );
}