using System.Text;

using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Generators;

internal sealed class TxtFileGenerator : FileGeneratorBase<TxtEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Txt;

  public override async Task<GeneratedFile> GenerateAsync(TxtEvidenceRequest request, CancellationToken ct = default)
  {
    var stream = new MemoryStream();
    var bytes = Encoding.UTF8.GetBytes(request.Text);
    await stream.WriteAsync(bytes.AsMemory(0, bytes.Length), ct);
    stream.Position = 0;

    return new GeneratedFile(stream, request.GetNormalizedFileName(), "text/plain");
  }
}