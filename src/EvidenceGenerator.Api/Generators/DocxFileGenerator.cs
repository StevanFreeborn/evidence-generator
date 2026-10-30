using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Generators;

internal sealed class DocxFileGenerator : FileGeneratorBase<DocxEvidenceRequest>
{
  public const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

  public override SupportedFileType SupportedType => SupportedFileType.Docx;

  public override Task<GeneratedFile> GenerateAsync(DocxEvidenceRequest request, CancellationToken ct = default)
  {
    var stream = new MemoryStream();

    using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
    {
      var mainPart = doc.AddMainDocumentPart();
      mainPart.Document = new Document();
      var body = new Body();

      var titlePara = new Paragraph(
        new ParagraphProperties(
          new ParagraphStyleId { Val = "Title" }
        ),
        new Run(
          new RunProperties(new Bold()),
          new Text(request.Title)
        )
      );

      body.AppendChild(titlePara);

      if (request.Sections is not null)
      {
        foreach (var section in request.Sections)
        {
          ct.ThrowIfCancellationRequested();

          if (string.IsNullOrWhiteSpace(section.Heading) is false)
          {
            var headingPara = new Paragraph(
              new ParagraphProperties(
                new ParagraphStyleId { Val = "Heading1" }
              ),
              new Run(
                new RunProperties(new Bold()),
                new Text(section.Heading)
              )
            );

            body.AppendChild(headingPara);
          }

          if (string.IsNullOrEmpty(section.ParagraphText) is false)
          {
            var para = new Paragraph(
                new Run(new Text(section.ParagraphText))
            );

            body.AppendChild(para);
          }
        }
      }

      if (request.Table is not null)
      {
        var table = new Table();

        if (request.Table.Headers.Count > 0)
        {
          var headerRow = new TableRow();

          foreach (var header in request.Table.Headers)
          {
            var cell = new TableCell(
              new Paragraph(
                new Run(
                  new RunProperties(new Bold()),
                  new Text(header ?? string.Empty)
                )
              )
            );

            headerRow.AppendChild(cell);
          }

          table.AppendChild(headerRow);
        }

        if (request.Table.Rows is not null)
        {
          foreach (var row in request.Table.Rows)
          {
            ct.ThrowIfCancellationRequested();

            var tableRow = new TableRow();

            foreach (var cellText in row)
            {
              var cell = new TableCell(
                new Paragraph(
                  new Run(new Text(cellText ?? string.Empty))
                )
              );

              tableRow.AppendChild(cell);
            }

            table.AppendChild(tableRow);
          }
        }

        body.AppendChild(table);
      }

      mainPart.Document.AppendChild(body);
      mainPart.Document.Save();
    }

    stream.Position = 0;

    return Task.FromResult(new GeneratedFile(stream, request.GetNormalizedFileName(), DocxContentType));
  }
}