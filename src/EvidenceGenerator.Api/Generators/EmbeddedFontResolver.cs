using PdfSharpCore.Fonts;

namespace EvidenceGenerator.Api.Generators;

internal sealed class EmbeddedFontResolver : IFontResolver
{
  private static readonly Lazy<byte[]> LazyFontData = new(LoadFontData);

  private static byte[] LoadFontData()
  {
    var assembly = typeof(EmbeddedFontResolver).Assembly;
    var resourceName = assembly.GetManifestResourceNames()
      .FirstOrDefault(n => n.EndsWith("LiberationSans-Regular.ttf", StringComparison.OrdinalIgnoreCase))
      ?? throw new InvalidOperationException("Embedded font LiberationSans-Regular.ttf was not found.");

    using var stream = assembly.GetManifestResourceStream(resourceName)
      ?? throw new InvalidOperationException($"Could not open resource stream for {resourceName}.");

    using var ms = new MemoryStream();

    stream.CopyTo(ms);

    return ms.ToArray();
  }

  public string DefaultFontName => "LiberationSans";

  public byte[] GetFont(string faceName)
  {
    return LazyFontData.Value;
  }

  public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
  {
    return new FontResolverInfo("LiberationSans");
  }
}