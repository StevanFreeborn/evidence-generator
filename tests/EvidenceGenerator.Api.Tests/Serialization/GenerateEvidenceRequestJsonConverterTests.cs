using System.Text.Json;

using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Serialization;

namespace EvidenceGenerator.Api.Tests.Serialization;

public class GenerateEvidenceRequestJsonConverterTests
{
  private readonly JsonSerializerOptions _options;

  public GenerateEvidenceRequestJsonConverterTests()
  {
    _options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };
    _options.Converters.Add(new GenerateEvidenceRequestJsonConverter());
  }

  [Theory]
  [InlineData("txt", typeof(TxtEvidenceRequest))]
  [InlineData("TXT", typeof(TxtEvidenceRequest))]
  [InlineData("Txt", typeof(TxtEvidenceRequest))]
  [InlineData("html", typeof(HtmlEvidenceRequest))]
  [InlineData("HTML", typeof(HtmlEvidenceRequest))]
  [InlineData("Html", typeof(HtmlEvidenceRequest))]
  [InlineData("csv", typeof(CsvEvidenceRequest))]
  [InlineData("CSV", typeof(CsvEvidenceRequest))]
  [InlineData("Csv", typeof(CsvEvidenceRequest))]
  [InlineData("xlsx", typeof(XlsxEvidenceRequest))]
  [InlineData("XLSX", typeof(XlsxEvidenceRequest))]
  [InlineData("Xlsx", typeof(XlsxEvidenceRequest))]
  [InlineData("docx", typeof(DocxEvidenceRequest))]
  [InlineData("DOCX", typeof(DocxEvidenceRequest))]
  [InlineData("Docx", typeof(DocxEvidenceRequest))]
  [InlineData("pptx", typeof(PptxEvidenceRequest))]
  [InlineData("PPTX", typeof(PptxEvidenceRequest))]
  [InlineData("Pptx", typeof(PptxEvidenceRequest))]
  [InlineData("pdf", typeof(PdfEvidenceRequest))]
  [InlineData("PDF", typeof(PdfEvidenceRequest))]
  [InlineData("Pdf", typeof(PdfEvidenceRequest))]
  [InlineData("json", typeof(JsonEvidenceRequest))]
  [InlineData("JSON", typeof(JsonEvidenceRequest))]
  [InlineData("Json", typeof(JsonEvidenceRequest))]
  [InlineData("jsonl", typeof(JsonlEvidenceRequest))]
  [InlineData("JSONL", typeof(JsonlEvidenceRequest))]
  [InlineData("Jsonl", typeof(JsonlEvidenceRequest))]
  public void Deserialize_AllSupportedFileTypes_ReturnsExpectedDerivedType(string fileType, Type expectedType)
  {
    var json = fileType.ToLowerInvariant() switch
    {
      "txt" => $$"""{"fileType":"{{fileType}}","appId":1,"attachmentFieldId":2,"fileName":"test.txt","text":"hello"}""",
      "html" => $$"""{"fileType":"{{fileType}}","appId":1,"attachmentFieldId":2,"fileName":"test.html","title":"Title","markdownOrHtmlBody":"# Hello"}""",
      "csv" => $$"""{"fileType":"{{fileType}}","appId":1,"attachmentFieldId":2,"fileName":"test.csv","headers":["A","B"],"rows":[["1","2"]]}""",
      "xlsx" => $$"""{"fileType":"{{fileType}}","appId":1,"attachmentFieldId":2,"fileName":"test.xlsx","sheets":[{"sheetName":"S1","columns":["A"],"rows":[["1"]]}]}""",
      "docx" => $$"""{"fileType":"{{fileType}}","appId":1,"attachmentFieldId":2,"fileName":"test.docx","title":"Doc","sections":[{"heading":"H1","paragraphText":"P1"}]}""",
      "pptx" => $$"""{"fileType":"{{fileType}}","appId":1,"attachmentFieldId":2,"fileName":"test.pptx","slides":[{"title":"Slide 1","bulletPoints":["Point 1"]}]}""",
      "pdf" => $$"""{"fileType":"{{fileType}}","appId":1,"attachmentFieldId":2,"fileName":"test.pdf","title":"Pdf","sections":[{"heading":"H1","body":"B1"}]}""",
      "json" => $$"""{"fileType":"{{fileType}}","appId":1,"attachmentFieldId":2,"fileName":"test.json","json":"{\"key\":\"value\"}"}""",
      "jsonl" => $$"""{"fileType":"{{fileType}}","appId":1,"attachmentFieldId":2,"fileName":"test.jsonl","text":"{\"key\":\"value\"}\n"}""",
      _ => throw new ArgumentException("Unknown type", nameof(fileType))
    };

    var result = JsonSerializer.Deserialize<GenerateEvidenceRequest>(json, _options);

    Assert.NotNull(result);
    Assert.IsType(expectedType, result);
    Assert.Equal(1, result.AppId);
    Assert.Equal(2, result.AttachmentFieldId);
  }

  [Fact]
  public void Deserialize_FileTypeAsLastProperty_DeserializesSuccessfully()
  {
    var json = """
      {
        "appId": 10,
        "attachmentFieldId": 20,
        "fileName": "sample.txt",
        "text": "sample text",
        "notes": "some notes",
        "fileType": "txt"
      }
    """;

    var result = JsonSerializer.Deserialize<GenerateEvidenceRequest>(json, _options);

    Assert.NotNull(result);

    var txtRequest = Assert.IsType<TxtEvidenceRequest>(result);

    Assert.Equal("sample text", txtRequest.Text);
    Assert.Equal("some notes", txtRequest.Notes);
  }

  [Theory]
  [InlineData("""{"appId":1,"attachmentFieldId":2,"fileName":"test.txt","text":"hello"}""")] // missing fileType
  [InlineData("""{"fileType":null,"appId":1,"attachmentFieldId":2,"fileName":"test.txt","text":"hello"}""")] // null fileType
  [InlineData("""{"fileType":"png","appId":1,"attachmentFieldId":2,"fileName":"test.png"}""")] // unsupported fileType
  [InlineData("""{"fileType":"unknown","appId":1,"attachmentFieldId":2,"fileName":"test.xyz"}""")] // unsupported fileType
  public void Deserialize_MissingNullOrUnsupportedFileType_ThrowsJsonException(string json)
  {
    Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<GenerateEvidenceRequest>(json, _options));
  }

  [Fact]
  public void Deserialize_MissingRequiredSubtypeProperty_ThrowsJsonException()
  {
    var json = """
      {
        "fileType": "pptx",
        "appId": 1,
        "attachmentFieldId": 2,
        "fileName": "presentation.pptx"
      }
    """;

    Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<GenerateEvidenceRequest>(json, _options));
  }
}