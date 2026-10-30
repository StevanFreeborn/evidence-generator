namespace EvidenceGenerator.Api.Onspring;

internal sealed class OnspringOptions
{
  public const string SectionName = "Onspring";
  public const string DefaultBaseAddress = "https://api.onspring.com/";

  public Uri BaseAddress { get; set; } = new(DefaultBaseAddress);
}