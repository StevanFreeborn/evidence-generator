using System.Net;
using System.Text;

using EvidenceGenerator.Api.Requests;

using Markdig;

namespace EvidenceGenerator.Api.Generators;

internal sealed class HtmlFileGenerator : FileGeneratorBase<HtmlEvidenceRequest>
{
  public override SupportedFileType SupportedType => SupportedFileType.Html;

  public override async Task<GeneratedFile> GenerateAsync(HtmlEvidenceRequest request, CancellationToken ct = default)
  {
    var bodyHtml = Markdown.ToHtml(request.MarkdownOrHtmlBody);
    var titleHtml = WebUtility.HtmlEncode(request.Title);

    var htmlDocument = $"""
      <!DOCTYPE html>
      <html lang="en">
      <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>{titleHtml}</title>
      </head>
      <body>
      {bodyHtml}
      </body>
      </html>
    """;

    var stream = new MemoryStream();
    var bytes = Encoding.UTF8.GetBytes(htmlDocument);
    await stream.WriteAsync(bytes.AsMemory(0, bytes.Length), ct);

    stream.Position = 0;

    return new GeneratedFile(stream, request.GetNormalizedFileName(), "text/html");
  }
}