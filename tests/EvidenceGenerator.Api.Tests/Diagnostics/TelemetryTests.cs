using System.Diagnostics;

using EvidenceGenerator.Api.Background;
using EvidenceGenerator.Api.Diagnostics;
using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Onspring;
using EvidenceGenerator.Api.Requests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace EvidenceGenerator.Api.Tests.Diagnostics;

public class TelemetryTests
{
  [Fact]
  public async Task Worker_ProcessEvidenceJob_CreatesActivityWithCorrectNameAndTags()
  {
    var activityTcs = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
    var jobId = Guid.NewGuid();

    using var listener = new ActivityListener
    {
      ShouldListenTo = s => s.Name is TelemetryConstants.ActivitySourceName,
      Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
      ActivityStopped = a =>
      {
        if (
          a.OperationName is TelemetryConstants.Activities.ProcessEvidenceJob &&
          a.GetTagItem(TelemetryConstants.Tags.JobId)?.ToString() == jobId.ToString()
        )
        {
          activityTcs.TrySetResult(a);
        }
      }
    };

    ActivitySource.AddActivityListener(listener);

    var request = new TxtEvidenceRequest
    {
      AppId = 123,
      AttachmentFieldId = 789,
      FileName = "evidence.txt",
      Text = "Hello tracing"
    };

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var mockQueue = Substitute.For<IBackgroundJobQueue>();
    var dequeuedFirst = false;

    mockQueue.DequeueAsync(Arg.Any<CancellationToken>())
      .Returns<ValueTask<EvidenceJob>>(async (callInfo) =>
      {
        var ct = callInfo.Arg<CancellationToken>();

        if (dequeuedFirst is false)
        {
          dequeuedFirst = true;
          return new EvidenceJob(jobId, "test-key", request);
        }

        await Task.Delay(Timeout.Infinite, ct);

        return null!;
      });

    var mockDispatcher = Substitute.For<IFileGeneratorDispatcher>();
    var generatedFile = new GeneratedFile(new MemoryStream(new byte[100]), "text/plain", "evidence.txt");

    mockDispatcher.GenerateAsync(request, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(generatedFile));

    var mockUploader = Substitute.For<IOnspringEvidenceUploader>();

    mockUploader.UploadAsync("test-key", request, generatedFile, Arg.Any<CancellationToken>())
      .Returns(Task.CompletedTask);

    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton(mockDispatcher);
    serviceCollection.AddSingleton(mockUploader);

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

    var logger = Substitute.For<ILogger<EvidenceProcessorWorker>>();
    var worker = new EvidenceProcessorWorker(mockQueue, scopeFactory, logger);

    await worker.StartAsync(cts.Token);
    var activity = await activityTcs.Task.WaitAsync(TimeSpan.FromSeconds(3));
    await worker.StopAsync(CancellationToken.None);

    Assert.NotNull(activity);
    Assert.Equal("123", activity.GetTagItem(TelemetryConstants.Tags.AppId)?.ToString());
    Assert.Equal("txt", activity.GetTagItem(TelemetryConstants.Tags.FileType)?.ToString());
    Assert.Equal("evidence.txt", activity.GetTagItem(TelemetryConstants.Tags.FileName)?.ToString());
    Assert.Equal(ActivityStatusCode.Ok, activity.Status);
  }

  [Fact]
  public async Task Worker_ProcessEvidenceJobFailure_RecordsExceptionAndSetsStatusToError()
  {
    var activityTcs = new TaskCompletionSource<Activity>(TaskCreationOptions.RunContinuationsAsynchronously);
    var jobId = Guid.NewGuid();

    using var listener = new ActivityListener
    {
      ShouldListenTo = s => s.Name is TelemetryConstants.ActivitySourceName,
      Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded,
      ActivityStopped = a =>
      {
        if (
          a.OperationName is TelemetryConstants.Activities.ProcessEvidenceJob &&
          a.GetTagItem(TelemetryConstants.Tags.JobId)?.ToString() == jobId.ToString()
        )
        {
          activityTcs.TrySetResult(a);
        }
      }
    };

    ActivitySource.AddActivityListener(listener);

    var request = new TxtEvidenceRequest
    {
      AppId = 123,
      AttachmentFieldId = 789,
      FileName = "error.txt",
      Text = "Fail"
    };

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var mockQueue = Substitute.For<IBackgroundJobQueue>();
    var dequeuedFirst = false;
    mockQueue.DequeueAsync(Arg.Any<CancellationToken>())
      .Returns<ValueTask<EvidenceJob>>(async (callInfo) =>
      {
        var ct = callInfo.Arg<CancellationToken>();

        if (dequeuedFirst is false)
        {
          dequeuedFirst = true;
          return new EvidenceJob(jobId, "test-key", request);
        }

        await Task.Delay(Timeout.Infinite, ct);

        return null!;
      });

    var mockDispatcher = Substitute.For<IFileGeneratorDispatcher>();

    mockDispatcher.GenerateAsync(request, Arg.Any<CancellationToken>())
      .ThrowsAsync(new InvalidOperationException("Generation failed"));

    var mockUploader = Substitute.For<IOnspringEvidenceUploader>();

    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton(mockDispatcher);
    serviceCollection.AddSingleton(mockUploader);

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

    var logger = Substitute.For<ILogger<EvidenceProcessorWorker>>();
    var worker = new EvidenceProcessorWorker(mockQueue, scopeFactory, logger);

    await worker.StartAsync(cts.Token);
    var activity = await activityTcs.Task.WaitAsync(TimeSpan.FromSeconds(3));
    await worker.StopAsync(CancellationToken.None);

    Assert.NotNull(activity);
    Assert.Equal(ActivityStatusCode.Error, activity.Status);
    Assert.Contains(activity.Events, e => e.Name == "exception");
  }
}