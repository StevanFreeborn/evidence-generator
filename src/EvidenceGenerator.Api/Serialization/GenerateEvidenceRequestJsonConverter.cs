using System.Text.Json;
using System.Text.Json.Serialization;

using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Serialization;

internal sealed class GenerateEvidenceRequestJsonConverter : JsonConverter<GenerateEvidenceRequest>
{
  public override GenerateEvidenceRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    using var doc = JsonDocument.ParseValue(ref reader);
    var root = doc.RootElement;

    if (root.ValueKind is not JsonValueKind.Object)
    {
      throw new JsonException("JSON payload must be an object.");
    }

    JsonProperty? fileTypeProperty = null;

    foreach (var property in root.EnumerateObject())
    {
      if (string.Equals(property.Name, "fileType", StringComparison.OrdinalIgnoreCase))
      {
        fileTypeProperty = property;
        break;
      }
    }

    if (fileTypeProperty is null || fileTypeProperty.Value.Value.ValueKind is not JsonValueKind.String)
    {
      throw new JsonException("The 'fileType' property is required and must be a string.");
    }

    var rawFileType = fileTypeProperty.Value.Value.GetString();

    if (string.IsNullOrWhiteSpace(rawFileType) || Enum.TryParse<SupportedFileType>(rawFileType, ignoreCase: true, out var fileType) is false)
    {
      throw new JsonException($"Unsupported or invalid 'fileType': '{rawFileType}'.");
    }

    var innerOptions = new JsonSerializerOptions(options);

    for (var i = innerOptions.Converters.Count - 1; i >= 0; i--)
    {
      if (innerOptions.Converters[i] is GenerateEvidenceRequestJsonConverter)
      {
        innerOptions.Converters.RemoveAt(i);
      }
    }

    var targetType = fileType switch
    {
      SupportedFileType.Txt => typeof(TxtEvidenceRequest),
      SupportedFileType.Html => typeof(HtmlEvidenceRequest),
      SupportedFileType.Csv => typeof(CsvEvidenceRequest),
      SupportedFileType.Xlsx => typeof(XlsxEvidenceRequest),
      SupportedFileType.Docx => typeof(DocxEvidenceRequest),
      SupportedFileType.Pptx => typeof(PptxEvidenceRequest),
      SupportedFileType.Pdf => typeof(PdfEvidenceRequest),
      _ => throw new JsonException($"Unsupported file type: {fileType}")
    };

    var result = root.Deserialize(targetType, innerOptions) as GenerateEvidenceRequest;

    return result ?? throw new JsonException($"Failed to deserialize request into {targetType.Name}.");
  }

  public override void Write(Utf8JsonWriter writer, GenerateEvidenceRequest value, JsonSerializerOptions options)
  {
    var innerOptions = new JsonSerializerOptions(options);

    for (var i = innerOptions.Converters.Count - 1; i >= 0; i--)
    {
      if (innerOptions.Converters[i] is GenerateEvidenceRequestJsonConverter)
      {
        innerOptions.Converters.RemoveAt(i);
      }
    }

    JsonSerializer.Serialize(writer, value, value.GetType(), innerOptions);
  }
}