using EvidenceGenerator.Api.Requests;

namespace EvidenceGenerator.Api.Background;

internal record EvidenceJob(
  Guid JobId,
  string ApiKey,
  GenerateEvidenceRequest Request,
  string? TraceParent = null,
  string? TraceState = null
);