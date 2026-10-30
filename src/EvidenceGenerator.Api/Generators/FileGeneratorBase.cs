using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Generators;

internal abstract class FileGeneratorBase<TRequest> : IFileGenerator<TRequest>
    where TRequest : GenerateEvidenceRequest
{
  public abstract SupportedFileType SupportedType { get; }

  public abstract Task<GeneratedFile> GenerateAsync(TRequest request, CancellationToken ct = default);

  Task<GeneratedFile> IFileGenerator.GenerateAsync(GenerateEvidenceRequest request, CancellationToken ct)
  {
    if (request is not TRequest typedRequest)
    {
      throw new ArgumentException(
        $"Expected request of type {typeof(TRequest).Name}, but received {request?.GetType().Name}.",
        nameof(request)
      );
    }

    return GenerateAsync(typedRequest, ct);
  }
}