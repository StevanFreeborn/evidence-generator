namespace EvidenceGenerator.Api.Validation;

internal sealed class ValidationResultBuilder
{
  private readonly Dictionary<string, List<string>> _errors = [];

  public void AddError(string propertyName, string errorMessage)
  {
    if (_errors.TryGetValue(propertyName, out var list) is false)
    {
      list = [];
      _errors[propertyName] = list;
    }

    list.Add(errorMessage);
  }

  public IDictionary<string, string[]> ToDictionary()
  {
    return _errors.ToDictionary(k => k.Key, v => v.Value.ToArray());
  }
}