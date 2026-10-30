using EvidenceGenerator.Api.Diagnostics;
using EvidenceGenerator.Api.Endpoints;
using EvidenceGenerator.Api.Extensions;
using EvidenceGenerator.Api.Generators;

var builder = WebApplication.CreateBuilder(args);

PdfFileGenerator.EnsureFontResolver();

builder.AddObservability();

builder.Services
  .AddEvidenceAuthentication()
  .AddEvidenceSerialization()
  .AddFileGenerators()
  .AddEvidenceValidation()
  .AddOnspringServices(builder.Configuration)
  .AddEvidenceBackgroundProcessing()
  .AddEvidenceProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapGenerateEndpoints();
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();