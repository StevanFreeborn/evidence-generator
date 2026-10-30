using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

using Onspring.API.SDK.Models;

namespace EvidenceGenerator.Api.Onspring;

internal sealed class OnspringEvidenceUploader(
  IOnspringClientFactory clientFactory,
  IRecordFieldValueMapper recordFieldValueMapper
) : IOnspringEvidenceUploader
{
  private readonly IOnspringClientFactory _clientFactory = clientFactory;
  private readonly IRecordFieldValueMapper _recordFieldValueMapper = recordFieldValueMapper;

  public async Task UploadAsync(
    string apiKey,
    GenerateEvidenceRequest request,
    GeneratedFile generatedFile,
    CancellationToken ct = default
  )
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
    ArgumentNullException.ThrowIfNull(request);
    ArgumentNullException.ThrowIfNull(generatedFile);

    var client = _clientFactory.CreateClient(apiKey);

    var record = new ResultRecord
    {
      AppId = request.AppId
    };

    if (request.RecordFields is not null)
    {
      foreach (var kvp in request.RecordFields)
      {
        record.FieldData.Add(_recordFieldValueMapper.Map(kvp.Key, kvp.Value));
      }
    }

    var recordResponse = await client.SaveRecordAsync(record);

    if (recordResponse.IsSuccessful is false || recordResponse.Value is null)
    {
      throw new InvalidOperationException($"Failed to save record in Onspring. Status: {recordResponse.StatusCode}. Message: {recordResponse.Message}");
    }

    var recordId = recordResponse.Value.Id;

    using var saveFileRequest = new SaveFileRequest
    {
      RecordId = recordId,
      FieldId = request.AttachmentFieldId,
      FileName = generatedFile.FileName,
      ContentType = generatedFile.ContentType,
      Notes = request.Notes,
      FileStream = generatedFile.Stream
    };

    var fileResponse = await client.SaveFileAsync(saveFileRequest);

    if (fileResponse.IsSuccessful is false)
    {
      throw new InvalidOperationException($"Failed to save file in Onspring. Status: {fileResponse.StatusCode}. Message: {fileResponse.Message}");
    }
  }
}