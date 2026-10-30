using System.Text.Json;

using EvidenceGenerator.Api.Auth;
using EvidenceGenerator.Api.Background;
using EvidenceGenerator.Api.Generators;
using EvidenceGenerator.Api.Onspring;
using EvidenceGenerator.Api.Serialization;
using EvidenceGenerator.Api.Validation;
using EvidenceGenerator.Api.Validation.Validators;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Json;

namespace EvidenceGenerator.Api.Extensions;

internal static class ServiceCollectionExtensions
{
  public static IServiceCollection AddEvidenceAuthentication(this IServiceCollection services)
  {
    services
      .AddAuthentication(OnspringApiKeyAuthenticationDefaults.SchemeName)
      .AddScheme<AuthenticationSchemeOptions, OnspringApiKeyAuthenticationHandler>(
        OnspringApiKeyAuthenticationDefaults.SchemeName,
        _ => { }
      );

    services.AddAuthorization();

    return services;
  }

  public static IServiceCollection AddEvidenceSerialization(this IServiceCollection services)
  {
    services.Configure<JsonOptions>(options =>
    {
      options.SerializerOptions.Converters.Add(new GenerateEvidenceRequestJsonConverter());
    });

    return services;
  }

  public static IServiceCollection AddFileGenerators(this IServiceCollection services)
  {
    services.AddSingleton<IFileGenerator, TxtFileGenerator>();
    services.AddSingleton<IFileGenerator, HtmlFileGenerator>();
    services.AddSingleton<IFileGenerator, CsvFileGenerator>();
    services.AddSingleton<IFileGenerator, XlsxFileGenerator>();
    services.AddSingleton<IFileGenerator, DocxFileGenerator>();
    services.AddSingleton<IFileGenerator, PptxFileGenerator>();
    services.AddSingleton<IFileGenerator, PdfFileGenerator>();
    services.AddSingleton<IFileGeneratorDispatcher, FileGeneratorDispatcher>();

    return services;
  }

  public static IServiceCollection AddEvidenceValidation(this IServiceCollection services)
  {
    services.AddSingleton<IEvidenceTypeValidator, TxtEvidenceRequestValidator>();
    services.AddSingleton<IEvidenceTypeValidator, HtmlEvidenceRequestValidator>();
    services.AddSingleton<IEvidenceTypeValidator, CsvEvidenceRequestValidator>();
    services.AddSingleton<IEvidenceTypeValidator, XlsxEvidenceRequestValidator>();
    services.AddSingleton<IEvidenceTypeValidator, DocxEvidenceRequestValidator>();
    services.AddSingleton<IEvidenceTypeValidator, PptxEvidenceRequestValidator>();
    services.AddSingleton<IEvidenceTypeValidator, PdfEvidenceRequestValidator>();
    services.AddSingleton<IEvidenceRequestValidator, EvidenceRequestValidatorDispatcher>();

    return services;
  }

  public static IServiceCollection AddOnspringServices(this IServiceCollection services, IConfiguration? configuration = null)
  {
    var options = configuration?.GetSection(OnspringOptions.SectionName).Get<OnspringOptions>()
      ?? new OnspringOptions();

    services.AddHttpClient<IOnspringClientFactory, OnspringClientFactory>(client =>
    {
      client.BaseAddress = options.BaseAddress;
    });

    services.AddSingleton<IRecordFieldValueMapper, RecordFieldValueMapper>();
    services.AddScoped<IOnspringEvidenceUploader, OnspringEvidenceUploader>();

    return services;
  }

  public static IServiceCollection AddEvidenceBackgroundProcessing(this IServiceCollection services)
  {
    services.AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>();
    services.AddHostedService<EvidenceProcessorWorker>();

    return services;
  }

  public static IServiceCollection AddEvidenceProblemDetails(this IServiceCollection services)
  {
    services.AddProblemDetails(options =>
    {
      options.CustomizeProblemDetails = context =>
      {
        var exception = context.HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error
          ?? context.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

        var badRequestTitle = "Bad Request";

        if (exception is BadHttpRequestException badHttpEx)
        {
          context.ProblemDetails.Status = badHttpEx.StatusCode;
          context.ProblemDetails.Title = badRequestTitle;
          context.ProblemDetails.Detail = badHttpEx.Message;
          context.HttpContext.Response.StatusCode = badHttpEx.StatusCode;
        }
        else if (exception is JsonException jsonEx)
        {
          context.ProblemDetails.Status = StatusCodes.Status400BadRequest;
          context.ProblemDetails.Title = badRequestTitle;
          context.ProblemDetails.Detail = jsonEx.Message;
          context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
      };
    });

    return services;
  }
}