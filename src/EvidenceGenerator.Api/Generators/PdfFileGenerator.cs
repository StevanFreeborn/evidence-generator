using EvidenceGenerator.Api.Requests;

using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;

namespace EvidenceGenerator.Api.Generators;

internal sealed class PdfFileGenerator : FileGeneratorBase<PdfEvidenceRequest>
{
  public const string PdfContentType = "application/pdf";
  private static readonly Lock SyncLock = new();
  private static bool s_fontResolverInitialized;

  public PdfFileGenerator()
  {
    EnsureFontResolver();
  }

  public static void EnsureFontResolver()
  {
    if (s_fontResolverInitialized)
    {
      return;
    }

    lock (SyncLock)
    {
      if (s_fontResolverInitialized is false)
      {
        GlobalFontSettings.FontResolver ??= new EmbeddedFontResolver();

        s_fontResolverInitialized = true;
      }
    }
  }

  public override SupportedFileType SupportedType => SupportedFileType.Pdf;

  public override Task<GeneratedFile> GenerateAsync(PdfEvidenceRequest request, CancellationToken ct = default)
  {
    EnsureFontResolver();

    using var document = new PdfDocument();
    var page = document.AddPage();
    var gfx = XGraphics.FromPdfPage(page);

    try
    {
      var titleFont = new XFont("LiberationSans", 18, XFontStyle.Bold);
      var headingFont = new XFont("LiberationSans", 14, XFontStyle.Bold);
      var bodyFont = new XFont("LiberationSans", 11, XFontStyle.Regular);

      double margin = 40;
      double yPos = 40;
      var contentWidth = page.Width.Point - (margin * 2);

      gfx.DrawString(request.Title, titleFont, XBrushes.Black, new XRect(margin, yPos, contentWidth, 24), XStringFormats.TopLeft);
      yPos += 35;

      if (request.Sections is not null)
      {
        foreach (var section in request.Sections)
        {
          ct.ThrowIfCancellationRequested();

          if (string.IsNullOrWhiteSpace(section.Heading) is false)
          {
            if (yPos > page.Height.Point - 60)
            {
              gfx.Dispose();
              page = document.AddPage();
              gfx = XGraphics.FromPdfPage(page);
              yPos = margin;
            }

            gfx.DrawString(section.Heading, headingFont, XBrushes.Black, new XRect(margin, yPos, contentWidth, 20), XStringFormats.TopLeft);
            yPos += 25;
          }

          if (string.IsNullOrEmpty(section.Body) is false)
          {
            if (yPos > page.Height.Point - 60)
            {
              gfx.Dispose();
              page = document.AddPage();
              gfx = XGraphics.FromPdfPage(page);
              yPos = margin;
            }

            gfx.DrawString(section.Body, bodyFont, XBrushes.Black, new XRect(margin, yPos, contentWidth, 18), XStringFormats.TopLeft);
            yPos += 25;
          }
        }
      }
    }
    finally
    {
      gfx.Dispose();
    }

    var stream = new MemoryStream();
    document.Save(stream, false);

    stream.Position = 0;

    return Task.FromResult(new GeneratedFile(stream, request.GetNormalizedFileName(), PdfContentType));
  }
}