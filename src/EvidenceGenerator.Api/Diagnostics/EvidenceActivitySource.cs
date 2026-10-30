using System.Diagnostics;

namespace EvidenceGenerator.Api.Diagnostics;

internal static class EvidenceActivitySource
{
  public static readonly ActivitySource Instance = new(TelemetryConstants.ActivitySourceName);
}