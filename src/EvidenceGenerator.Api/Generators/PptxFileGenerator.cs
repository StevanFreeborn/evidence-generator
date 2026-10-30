using EvidenceGenerator.Api.Requests;

using ShapeCrawler;

namespace EvidenceGenerator.Api.Generators;

internal sealed class PptxFileGenerator : FileGeneratorBase<PptxEvidenceRequest>
{
  public const string PptxContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";

  public override SupportedFileType SupportedType => SupportedFileType.Pptx;

  public override Task<GeneratedFile> GenerateAsync(PptxEvidenceRequest request, CancellationToken ct = default)
  {
    var stream = new MemoryStream();

    using var pres = new Presentation();

    foreach (var slideData in request.Slides)
    {
      ct.ThrowIfCancellationRequested();

      pres.Slides.Add(2);

      var slide = pres.Slides[^1];

      if (slide.Shapes.Count >= 2 && slide.Shapes[0].TextBox is { } titleBox && slide.Shapes[1].TextBox is { } contentBox)
      {
        titleBox.SetText(slideData.Title);

        var content = string.Join("\n", slideData.BulletPoints.Select(b => $"• {b}"));

        contentBox.SetText(content);
      }
      else
      {
        slide.Shapes.AddTextBox(50, 50, 600, 100, slideData.Title);

        var content = string.Join("\n", slideData.BulletPoints.Select(b => $"• {b}"));

        slide.Shapes.AddTextBox(50, 160, 600, 400, content);
      }
    }

    pres.Save(stream);

    stream.Position = 0;

    return Task.FromResult(new GeneratedFile(stream, request.GetNormalizedFileName(), PptxContentType));
  }
}