using Onspring.API.SDK;

namespace EvidenceGenerator.Api.Onspring;

internal sealed class OnspringClientFactory(HttpClient httpClient) : IOnspringClientFactory
{
  private readonly HttpClient _httpClient = httpClient;

  public IOnspringClient CreateClient(string apiKey)
  {
    return new OnspringClient(apiKey, _httpClient);
  }
}