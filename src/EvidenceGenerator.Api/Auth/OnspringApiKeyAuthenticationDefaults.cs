namespace EvidenceGenerator.Api.Auth;

internal static class OnspringApiKeyAuthenticationDefaults
{
  public const string SchemeName = "OnspringApiKey";
  public const string HeaderName = "X-Onspring-ApiKey";
  public const string ClaimType = "urn:onspring:apikey";
}