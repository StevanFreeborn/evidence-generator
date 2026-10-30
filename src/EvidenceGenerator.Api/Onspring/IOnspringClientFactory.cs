using Onspring.API.SDK;

namespace EvidenceGenerator.Api.Onspring;

internal interface IOnspringClientFactory
{
  IOnspringClient CreateClient(string apiKey);
}