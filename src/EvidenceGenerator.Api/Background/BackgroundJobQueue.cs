using System.Threading.Channels;

namespace EvidenceGenerator.Api.Background;

internal sealed class BackgroundJobQueue : IBackgroundJobQueue
{
  private readonly Channel<EvidenceJob> _channel;

  public BackgroundJobQueue(int capacity = 100)
  {
    var options = new BoundedChannelOptions(capacity)
    {
      FullMode = BoundedChannelFullMode.Wait
    };

    _channel = Channel.CreateBounded<EvidenceJob>(options);
  }

  public ValueTask EnqueueAsync(EvidenceJob job, CancellationToken ct = default)
  {
    ArgumentNullException.ThrowIfNull(job);
    return _channel.Writer.WriteAsync(job, ct);
  }

  public ValueTask<EvidenceJob> DequeueAsync(CancellationToken ct = default)
  {
    return _channel.Reader.ReadAsync(ct);
  }
}