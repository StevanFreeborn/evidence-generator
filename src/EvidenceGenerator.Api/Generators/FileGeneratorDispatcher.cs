using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Generators;

internal sealed class FileGeneratorDispatcher(IEnumerable<IFileGenerator> generators) : IFileGeneratorDispatcher
{
  private readonly Dictionary<SupportedFileType, IFileGenerator> _generators = generators.ToDictionary(g => g.SupportedType);

  public Task<GeneratedFile> GenerateAsync(GenerateEvidenceRequest request, CancellationToken ct = default)
  {
    ArgumentNullException.ThrowIfNull(request);

    if (_generators.TryGetValue(request.FileType, out var generator) is false)
    {
      throw new NotSupportedException($"No generator registered for file type '{request.FileType}'.");
    }

    return generator.GenerateAsync(request, ct);
  }
}