using System.Net;

using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Onspring;
using EvidenceGenerator.Api.Requests;

using NSubstitute;

using Onspring.API.SDK;
using Onspring.API.SDK.Models;

namespace EvidenceGenerator.Api.Tests.Onspring;

public class OnspringEvidenceUploaderTests
{
  private readonly IOnspringClientFactory _clientFactory;
  private readonly IOnspringClient _client;
  private readonly IRecordFieldValueMapper _mapper;
  private readonly OnspringEvidenceUploader _uploader;

  public OnspringEvidenceUploaderTests()
  {
    _clientFactory = Substitute.For<IOnspringClientFactory>();
    _client = Substitute.For<IOnspringClient>();
    _mapper = new RecordFieldValueMapper();
    _clientFactory.CreateClient(Arg.Any<string>()).Returns(_client);
    _uploader = new OnspringEvidenceUploader(_clientFactory, _mapper);
  }

  [Fact]
  public async Task UploadAsync_SuccessfulRecordAndFileSave_CallsOnspringClientCorrectly()
  {
    var apiKey = "test-api-key";
    var request = new TxtEvidenceRequest
    {
      AppId = 123,
      AttachmentFieldId = 456,
      FileName = "evidence.txt",
      Notes = "Test note",
      Text = "Content",
      RecordFields = new Dictionary<int, object?>
      {
        { 10, "Value 1" },
        { 20, 42 }
      }
    };

    var memoryStream = new MemoryStream();
    var generatedFile = new GeneratedFile(memoryStream, "evidence.txt", "text/plain");

    var recordResponse = new ApiResponse<SaveRecordResponse>
    {
      StatusCode = HttpStatusCode.Created,
      Value = new SaveRecordResponse { Id = 789 }
    };

    var fileResponse = new ApiResponse<CreatedWithIdResponse<int>>
    {
      StatusCode = HttpStatusCode.Created,
      Value = new CreatedWithIdResponse<int> { Id = 999 }
    };

    _client.SaveRecordAsync(Arg.Any<ResultRecord>()).Returns(Task.FromResult(recordResponse));
    _client.SaveFileAsync(Arg.Any<SaveFileRequest>()).Returns(Task.FromResult(fileResponse));

    await _uploader.UploadAsync(apiKey, request, generatedFile);

    _clientFactory.Received(1).CreateClient(apiKey);

    await _client.Received(1).SaveRecordAsync(Arg.Is<ResultRecord>(r =>
      r.AppId == 123 &&
      r.FieldData.Any(f => f.FieldId == 10) &&
      r.FieldData.Any(f => f.FieldId == 20)
    ));

    await _client.Received(1).SaveFileAsync(Arg.Is<SaveFileRequest>(f =>
      f.RecordId == 789 &&
      f.FieldId == 456 &&
      f.FileName == "evidence.txt" &&
      f.ContentType == "text/plain" &&
      f.Notes == "Test note" &&
      f.FileStream == memoryStream
    ));
  }

  [Fact]
  public async Task UploadAsync_RecordSaveFails_ThrowsInvalidOperationExceptionAndNeverCallsSaveFile()
  {
    var apiKey = "test-api-key";
    var request = new TxtEvidenceRequest
    {
      AppId = 123,
      AttachmentFieldId = 456,
      FileName = "evidence.txt",
      Text = "Content"
    };
    var generatedFile = new GeneratedFile(new MemoryStream(), "evidence.txt", "text/plain");

    var recordResponse = new ApiResponse<SaveRecordResponse>
    {
      StatusCode = HttpStatusCode.BadRequest,
      Message = "AppId does not exist."
    };

    _client.SaveRecordAsync(Arg.Any<ResultRecord>()).Returns(Task.FromResult(recordResponse));

    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _uploader.UploadAsync(apiKey, request, generatedFile));

    Assert.Contains("BadRequest", ex.Message);
    Assert.Contains("AppId does not exist.", ex.Message);

    await _client.DidNotReceive().SaveFileAsync(Arg.Any<SaveFileRequest>());
  }

  [Fact]
  public async Task UploadAsync_FileSaveFails_ThrowsInvalidOperationException()
  {
    var apiKey = "test-api-key";
    var request = new TxtEvidenceRequest
    {
      AppId = 123,
      AttachmentFieldId = 456,
      FileName = "evidence.txt",
      Text = "Content"
    };
    var generatedFile = new GeneratedFile(new MemoryStream(), "evidence.txt", "text/plain");

    var recordResponse = new ApiResponse<SaveRecordResponse>
    {
      StatusCode = HttpStatusCode.Created,
      Value = new SaveRecordResponse { Id = 789 }
    };

    var fileResponse = new ApiResponse<CreatedWithIdResponse<int>>
    {
      StatusCode = HttpStatusCode.Forbidden,
      Message = "Attachment size limit exceeded."
    };

    _client.SaveRecordAsync(Arg.Any<ResultRecord>()).Returns(Task.FromResult(recordResponse));
    _client.SaveFileAsync(Arg.Any<SaveFileRequest>()).Returns(Task.FromResult(fileResponse));

    var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _uploader.UploadAsync(apiKey, request, generatedFile));

    Assert.Contains("Forbidden", ex.Message);
    Assert.Contains("Attachment size limit exceeded.", ex.Message);
  }
}