using System.Text;

using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Tests.Generators;

public class HtmlFileGeneratorTests
{
  [Fact]
  public async Task GenerateAsync_MarkdownContent_RendersFullHtml5Document()
  {
    var generator = new HtmlFileGenerator();
    var request = new HtmlEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "report",
      Title = "My Report",
      MarkdownOrHtmlBody = "# Header\n\nThis is an *item*."
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("report.html", result.FileName);
    Assert.Equal("text/html", result.ContentType);
    Assert.Equal(0, result.Stream.Position);

    using var reader = new StreamReader(result.Stream, Encoding.UTF8, leaveOpen: true);
    var html = await reader.ReadToEndAsync();

    Assert.Contains("<!DOCTYPE html>", html);
    Assert.Contains("<title>My Report</title>", html);
    Assert.Contains("<h1>Header</h1>", html);
    Assert.Contains("<em>item</em>", html);
  }
}