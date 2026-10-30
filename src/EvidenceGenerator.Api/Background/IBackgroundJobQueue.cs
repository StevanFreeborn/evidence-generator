namespace EvidenceGenerator.Api.Background;

internal interface IBackgroundJobQueue
{
  ValueTask EnqueueAsync(EvidenceJob job, CancellationToken ct = default);
  ValueTask<EvidenceJob> DequeueAsync(CancellationToken ct = default);
}