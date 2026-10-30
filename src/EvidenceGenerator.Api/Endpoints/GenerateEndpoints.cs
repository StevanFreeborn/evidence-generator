using System.Diagnostics;
using System.Security.Claims;

using EvidenceGenerator.Api.Auth;
using EvidenceGenerator.Api.Background;
using EvidenceGenerator.Api.Requests;
using EvidenceGenerator.Api.Validation;

namespace EvidenceGenerator.Api.Endpoints;

internal static class GenerateEndpoints
{
  public static IEndpointRouteBuilder MapGenerateEndpoints(this IEndpointRouteBuilder endpoints)
  {
    endpoints
      .MapPost("/generate", async (
        GenerateEvidenceRequest request,
        ClaimsPrincipal user,
        IBackgroundJobQueue queue,
        IEvidenceRequestValidator validator
      ) =>
      {
        var errors = validator.Validate(request);

        if (errors.Count > 0)
        {
          return Results.ValidationProblem(errors);
        }

        var apiKey = user.FindFirstValue(OnspringApiKeyAuthenticationDefaults.ClaimType)!;
        var jobId = Guid.NewGuid();
        var currentActivity = Activity.Current;
        var job = new EvidenceJob(
          jobId,
          apiKey,
          request,
          TraceParent: currentActivity?.Id,
          TraceState: currentActivity?.TraceStateString
        );

        await queue.EnqueueAsync(job);

        return Results.Accepted(value: new
        {
          jobId,
          status = "Queued"
        });
      })
      .RequireAuthorization();

    return endpoints;
  }
}