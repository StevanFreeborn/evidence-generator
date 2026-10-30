namespace EvidenceGenerator.Api.Generators;

internal record GeneratedFile(
  Stream Stream,
  string FileName,
  string ContentType
) : IDisposable, IAsyncDisposable
{
  public void Dispose()
  {
    Stream.Dispose();
  }

  public ValueTask DisposeAsync()
  {
    return Stream.DisposeAsync();
  }
}