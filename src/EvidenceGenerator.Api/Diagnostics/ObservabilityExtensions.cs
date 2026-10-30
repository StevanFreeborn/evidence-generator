using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EvidenceGenerator.Api.Diagnostics;

internal static class ObservabilityExtensions
{
  public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
  {
    var seqOptions = builder.Configuration.GetSection(SeqOptions.SectionName).Get<SeqOptions>();

    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddOpenTelemetry(logging =>
    {
      logging.IncludeFormattedMessage = true;
      logging.IncludeScopes = true;

      var resourceBuilder = ResourceBuilder.CreateDefault()
        .AddService(TelemetryConstants.ServiceName);

      logging.SetResourceBuilder(resourceBuilder);

      if (string.IsNullOrWhiteSpace(seqOptions?.ServerUrl) is false)
      {
        var seqLogsEndpoint = new Uri(new Uri(seqOptions.ServerUrl.TrimEnd('/') + "/"), "ingest/otlp/v1/logs");

        logging.AddOtlpExporter(otlp =>
        {
          otlp.Endpoint = seqLogsEndpoint;
          otlp.Protocol = OtlpExportProtocol.HttpProtobuf;

          if (string.IsNullOrWhiteSpace(seqOptions.ApiKey) is false)
          {
            otlp.Headers = $"X-Seq-ApiKey={seqOptions.ApiKey}";
          }
        });
      }
    });

    builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(TelemetryConstants.ServiceName))
      .WithTracing(tracing =>
      {
        tracing.AddSource(TelemetryConstants.ActivitySourceName);
        tracing.AddAspNetCoreInstrumentation(options =>
        {
          options.RecordException = true;
        });

        tracing.AddHttpClientInstrumentation(options =>
        {
          options.RecordException = true;
        });

        if (string.IsNullOrWhiteSpace(seqOptions?.ServerUrl) is false)
        {
          var seqTracesEndpoint = new Uri(new Uri(seqOptions.ServerUrl.TrimEnd('/') + "/"), "ingest/otlp/v1/traces");

          tracing.AddOtlpExporter(otlp =>
          {
            otlp.Endpoint = seqTracesEndpoint;
            otlp.Protocol = OtlpExportProtocol.HttpProtobuf;

            if (string.IsNullOrWhiteSpace(seqOptions.ApiKey) is false)
            {
              otlp.Headers = $"X-Seq-ApiKey={seqOptions.ApiKey}";
            }
          });
        }
      });

    return builder;
  }
}