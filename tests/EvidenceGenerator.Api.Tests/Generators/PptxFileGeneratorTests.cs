using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

using ShapeCrawler;

namespace EvidenceGenerator.Api.Tests.Generators;

public class PptxFileGeneratorTests
{
  [Fact]
  public async Task GenerateAsync_SlidesWithTitleAndBullets_CreatesValidPresentation()
  {
    var generator = new PptxFileGenerator();
    var request = new PptxEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "presentation",
      Slides = [
        new PptxSlide(
          Title: "Quarterly Overview",
          BulletPoints: ["Completed audit", "Met SLA targets"]
        ),
        new PptxSlide(
          Title: "Next Steps",
          BulletPoints: ["Implement recommendations", "Follow up with team"]
        )
      ]
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("presentation.pptx", result.FileName);
    Assert.Equal("application/vnd.openxmlformats-officedocument.presentationml.presentation", result.ContentType);
    Assert.Equal(0, result.Stream.Position);

    var pres = new Presentation(result.Stream);

    Assert.Equal(2, pres.Slides.Count);

    var slide1 = pres.Slides[0];

    Assert.Contains(slide1.Shapes, s => s.TextBox != null && s.TextBox.Text.Contains("Quarterly Overview"));
    Assert.Contains(slide1.Shapes, s => s.TextBox != null && s.TextBox.Text.Contains("Completed audit"));

    var slide2 = pres.Slides[1];

    Assert.Contains(slide2.Shapes, s => s.TextBox != null && s.TextBox.Text.Contains("Next Steps"));
    Assert.Contains(slide2.Shapes, s => s.TextBox != null && s.TextBox.Text.Contains("Implement recommendations"));
  }
}