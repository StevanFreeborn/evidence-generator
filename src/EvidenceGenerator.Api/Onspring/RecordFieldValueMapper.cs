using System.Text.Json;

using Onspring.API.SDK.Models;

namespace EvidenceGenerator.Api.Onspring;

internal sealed class RecordFieldValueMapper : IRecordFieldValueMapper
{
  public RecordFieldValue Map(int fieldId, object? value)
  {
    if (value is null)
    {
      return new StringFieldValue(fieldId, null);
    }

    if (value is JsonElement element)
    {
      return FromJsonElement(fieldId, element);
    }

    return FromClrObject(fieldId, value);
  }

  private static RecordFieldValue FromJsonElement(int fieldId, JsonElement element)
  {
    switch (element.ValueKind)
    {
      case JsonValueKind.String:
        var str = element.GetString();

        if (Guid.TryParse(str, out var guid))
        {
          return new GuidFieldValue(fieldId, guid);
        }

        if (DateTime.TryParse(str, System.Globalization.CultureInfo.InvariantCulture, out var dt))
        {
          return new DateFieldValue(fieldId, dt);
        }

        return new StringFieldValue(fieldId, str);

      case JsonValueKind.Number:
        if (element.TryGetInt32(out var intVal))
        {
          return new IntegerFieldValue(fieldId, intVal);
        }

        if (element.TryGetDecimal(out var decVal))
        {
          return new DecimalFieldValue(fieldId, decVal);
        }

        return new StringFieldValue(fieldId, element.GetRawText());

      case JsonValueKind.True:
      case JsonValueKind.False:
        return new StringFieldValue(fieldId, element.GetBoolean().ToString());

      case JsonValueKind.Array:
        var stringList = new List<string>();
        var guidList = new List<Guid>();
        var intList = new List<int>();
        var isGuidList = true;
        var isIntList = true;

        foreach (var item in element.EnumerateArray())
        {
          if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out var num))
          {
            intList.Add(num);
            isGuidList = false;
          }
          else if (item.ValueKind == JsonValueKind.String)
          {
            var itemStr = item.GetString() ?? string.Empty;
            stringList.Add(itemStr);
            isIntList = false;

            if (isGuidList && Guid.TryParse(itemStr, out var itemGuid))
            {
              guidList.Add(itemGuid);
            }
            else
            {
              isGuidList = false;
            }
          }
          else
          {
            isGuidList = false;
            isIntList = false;
            stringList.Add(item.GetRawText());
          }
        }

        if (isIntList && intList.Count > 0)
        {
          return new IntegerListFieldValue(fieldId, intList);
        }

        if (isGuidList && guidList.Count > 0)
        {
          return new GuidListFieldValue(fieldId, guidList);
        }

        return new StringListFieldValue(fieldId, stringList);

      case JsonValueKind.Null:
      case JsonValueKind.Undefined:
      default:
        return new StringFieldValue(fieldId, null);
    }
  }

  private static RecordFieldValue FromClrObject(int fieldId, object value)
  {
    return value switch
    {
      string s when Guid.TryParse(s, out var g) => new GuidFieldValue(fieldId, g),
      string s when DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var dt) => new DateFieldValue(fieldId, dt),
      string s => new StringFieldValue(fieldId, s),
      int i => new IntegerFieldValue(fieldId, i),
      long l when l is >= int.MinValue and <= int.MaxValue => new IntegerFieldValue(fieldId, (int)l),
      decimal d => new DecimalFieldValue(fieldId, d),
      double dbl => new DecimalFieldValue(fieldId, (decimal)dbl),
      float f => new DecimalFieldValue(fieldId, (decimal)f),
      DateTime dt => new DateFieldValue(fieldId, dt),
      DateTimeOffset dto => new DateFieldValue(fieldId, dto.UtcDateTime),
      Guid g => new GuidFieldValue(fieldId, g),
      bool b => new StringFieldValue(fieldId, b.ToString()),
      IEnumerable<string> strings => new StringListFieldValue(fieldId, strings.ToList()),
      IEnumerable<Guid> guids => new GuidListFieldValue(fieldId, guids.ToList()),
      IEnumerable<int> ints => new IntegerListFieldValue(fieldId, ints.ToList()),
      RecordFieldValue rfv => rfv,
      _ => new StringFieldValue(fieldId, value.ToString())
    };
  }
}