using EvidenceGenerator.Api.Background;
using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Onspring;
using EvidenceGenerator.Api.Requests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvidenceGenerator.Api.Tests.Background;

public class EvidenceProcessorWorkerTests
{
  private sealed class TrackableMemoryStream : MemoryStream
  {
    public bool IsDisposed { get; private set; }

    protected override void Dispose(bool disposing)
    {
      IsDisposed = true;
      base.Dispose(disposing);
    }
  }

  [Fact]
  public async Task Worker_ProcessesJob_GeneratesAndUploadsFileAndDisposesStream()
  {
    var queue = new BackgroundJobQueue();
    var dispatcher = Substitute.For<IFileGeneratorDispatcher>();
    var uploader = Substitute.For<IOnspringEvidenceUploader>();
    var logger = Substitute.For<ILogger<EvidenceProcessorWorker>>();

    var stream = new TrackableMemoryStream();
    var generatedFile = new GeneratedFile(stream, "test.txt", "text/plain");

    dispatcher.GenerateAsync(Arg.Any<GenerateEvidenceRequest>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(generatedFile));

    var services = new ServiceCollection();
    services.AddSingleton(dispatcher);
    services.AddSingleton(uploader);

    var serviceProvider = services.BuildServiceProvider();
    var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

    var worker = new EvidenceProcessorWorker(queue, scopeFactory, logger);

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
    var workerTask = worker.StartAsync(cts.Token);

    var request = new TxtEvidenceRequest
    {
      AppId = 1,
      AttachmentFieldId = 2,
      FileName = "test.txt",
      Text = "Content"
    };
    var job = new EvidenceJob(Guid.NewGuid(), "api-key-123", request);

    await queue.EnqueueAsync(job);

    var delay = 0;

    while (stream.IsDisposed is false && delay < 1000)
    {
      await Task.Delay(50);
      delay += 50;
    }

    cts.Cancel();
    await workerTask;
    await dispatcher.Received(1).GenerateAsync(request, Arg.Any<CancellationToken>());
    await uploader.Received(1).UploadAsync("api-key-123", request, generatedFile, Arg.Any<CancellationToken>());

    Assert.True(stream.IsDisposed);
  }

  [Fact]
  public async Task Worker_UploadFails_DisposesStreamAndContinuesProcessingNextJob()
  {
    var queue = new BackgroundJobQueue();
    var dispatcher = Substitute.For<IFileGeneratorDispatcher>();
    var uploader = Substitute.For<IOnspringEvidenceUploader>();
    var logger = Substitute.For<ILogger<EvidenceProcessorWorker>>();

    var stream1 = new TrackableMemoryStream();
    var stream2 = new TrackableMemoryStream();
    var file1 = new GeneratedFile(stream1, "fail.txt", "text/plain");
    var file2 = new GeneratedFile(stream2, "success.txt", "text/plain");

    var request1 = new TxtEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "fail.txt", Text = "Fail" };
    var request2 = new TxtEvidenceRequest { AppId = 1, AttachmentFieldId = 2, FileName = "success.txt", Text = "Success" };

    dispatcher.GenerateAsync(request1, Arg.Any<CancellationToken>()).Returns(Task.FromResult(file1));
    dispatcher.GenerateAsync(request2, Arg.Any<CancellationToken>()).Returns(Task.FromResult(file2));

    uploader.UploadAsync("key1", request1, file1, Arg.Any<CancellationToken>())
        .ThrowsAsync(new InvalidOperationException("Onspring boom"));

    uploader.UploadAsync("key2", request2, file2, Arg.Any<CancellationToken>())
        .Returns(Task.CompletedTask);

    var services = new ServiceCollection();
    services.AddSingleton(dispatcher);
    services.AddSingleton(uploader);

    var serviceProvider = services.BuildServiceProvider();
    var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

    var worker = new EvidenceProcessorWorker(queue, scopeFactory, logger);

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
    var workerTask = worker.StartAsync(cts.Token);

    await queue.EnqueueAsync(new EvidenceJob(Guid.NewGuid(), "key1", request1));
    await queue.EnqueueAsync(new EvidenceJob(Guid.NewGuid(), "key2", request2));

    var delay = 0;

    while ((stream1.IsDisposed is false || stream2.IsDisposed is false) && delay < 1500)
    {
      await Task.Delay(50);
      delay += 50;
    }

    cts.Cancel();
    await workerTask;

    Assert.True(stream1.IsDisposed);
    Assert.True(stream2.IsDisposed);

    await uploader.Received(1).UploadAsync("key2", request2, file2, Arg.Any<CancellationToken>());
  }
}