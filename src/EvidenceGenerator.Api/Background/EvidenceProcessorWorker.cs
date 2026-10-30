using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using EvidenceGenerator.Api.Diagnostics;
using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Onspring;

namespace EvidenceGenerator.Api.Background;

internal sealed partial class EvidenceProcessorWorker(
  IBackgroundJobQueue queue,
  IServiceScopeFactory serviceScopeFactory,
  ILogger<EvidenceProcessorWorker> logger
) : BackgroundService
{
  private readonly IBackgroundJobQueue _queue = queue;
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
  private readonly ILogger<EvidenceProcessorWorker> _logger = logger;

  [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Processing evidence generation job {JobId} for App {AppId}, Field {FieldId}, Type {FileType}, File {FileName}.")]
  private static partial void LogProcessingJob(ILogger logger, Guid jobId, int appId, int fieldId, string fileType, string fileName);

  [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Successfully processed and uploaded evidence generation job {JobId} ({FileSizeBytes} bytes).")]
  private static partial void LogJobSuccess(ILogger logger, Guid jobId, long fileSizeBytes);

  [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to process evidence generation job {JobId} for App {AppId}, File {FileName}.")]
  private static partial void LogJobFailure(ILogger logger, Exception ex, Guid jobId, int appId, string fileName);

  [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "OpenTelemetry file type semantic convention is lowercase.")]
  [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The worker loop must log and record exceptions for the job rather than crashing the background worker service.")]
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (stoppingToken.IsCancellationRequested is false)
    {
      EvidenceJob job;

      try
      {
        job = await _queue.DequeueAsync(stoppingToken);

        if (job == null)
        {
          await Task.Delay(50, stoppingToken);
          continue;
        }
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        break;
      }

      using var scope = _serviceScopeFactory.CreateScope();
      var dispatcher = scope.ServiceProvider.GetRequiredService<IFileGeneratorDispatcher>();
      var uploader = scope.ServiceProvider.GetRequiredService<IOnspringEvidenceUploader>();

      using var activity = EvidenceActivitySource.Instance.StartActivity(
        TelemetryConstants.Activities.ProcessEvidenceJob,
        ActivityKind.Consumer,
        parentId: job.TraceParent
      );

      var fileTypeLower = job.Request.FileType.ToString().ToLowerInvariant();
      var normalizedFileName = job.Request.GetNormalizedFileName();

      if (activity is not null)
      {
        if (string.IsNullOrEmpty(job.TraceState) is false)
        {
          activity.TraceStateString = job.TraceState;
        }

        activity.SetTag(TelemetryConstants.Tags.JobId, job.JobId.ToString());
        activity.SetTag(TelemetryConstants.Tags.AppId, job.Request.AppId.ToString(CultureInfo.InvariantCulture));
        activity.SetTag(TelemetryConstants.Tags.FileType, fileTypeLower);
        activity.SetTag(TelemetryConstants.Tags.FileName, normalizedFileName);
        activity.SetTag(TelemetryConstants.Tags.FieldId, job.Request.AttachmentFieldId.ToString(CultureInfo.InvariantCulture));
      }

      LogProcessingJob(
        _logger,
        job.JobId,
        job.Request.AppId,
        job.Request.AttachmentFieldId,
        fileTypeLower,
        normalizedFileName
      );

      try
      {
        using var generatedFile = await dispatcher.GenerateAsync(job.Request, stoppingToken);

        var fileSizeBytes = generatedFile.Stream.Length;
        activity?.SetTag(TelemetryConstants.Tags.FileSizeBytes, fileSizeBytes);

        await uploader.UploadAsync(job.ApiKey, job.Request, generatedFile, stoppingToken);

        LogJobSuccess(_logger, job.JobId, fileSizeBytes);

        activity?.SetStatus(ActivityStatusCode.Ok);
      }
      catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
      {
        break;
      }
      catch (Exception ex)
      {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);

        LogJobFailure(
          _logger,
          ex,
          job.JobId,
          job.Request.AppId,
          normalizedFileName
        );
      }
    }
  }
}