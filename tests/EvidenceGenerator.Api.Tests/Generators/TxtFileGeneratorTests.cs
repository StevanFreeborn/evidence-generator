using System.Text;

using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Tests.Generators;

public class TxtFileGeneratorTests
{
  [Fact]
  public async Task GenerateAsync_ValidRequest_ReturnsExpectedGeneratedFile()
  {
    var generator = new TxtFileGenerator();
    var request = new TxtEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "evidence",
      Text = "Hello World! Special chars: € and ñ"
    };

    var result = await generator.GenerateAsync(request);

    Assert.NotNull(result);
    Assert.Equal("evidence.txt", result.FileName);
    Assert.Equal("text/plain", result.ContentType);
    Assert.Equal(0, result.Stream.Position);

    using var reader = new StreamReader(result.Stream, Encoding.UTF8, leaveOpen: true);
    var content = await reader.ReadToEndAsync();

    Assert.Equal(request.Text, content);
  }
}