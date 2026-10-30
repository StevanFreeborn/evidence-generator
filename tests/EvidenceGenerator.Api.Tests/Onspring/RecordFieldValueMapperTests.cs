using System.Text.Json;

using EvidenceGenerator.Api.Onspring;

using Onspring.API.SDK.Models;

namespace EvidenceGenerator.Api.Tests.Onspring;

public class RecordFieldValueMapperTests
{
  private readonly RecordFieldValueMapper _mapper = new();

  [Fact]
  public void Map_NullValue_ReturnsStringFieldValueWithNull()
  {
    var result = _mapper.Map(100, null);

    var stringField = Assert.IsType<StringFieldValue>(result);
    Assert.Equal(100, stringField.FieldId);
    Assert.Null(stringField.Value);
  }

  [Fact]
  public void Map_ClrString_ReturnsStringFieldValue()
  {
    var result = _mapper.Map(101, "hello world");

    var stringField = Assert.IsType<StringFieldValue>(result);
    Assert.Equal("hello world", stringField.Value);
  }

  [Fact]
  public void Map_ClrGuid_ReturnsGuidFieldValue()
  {
    var guid = Guid.NewGuid();
    var result = _mapper.Map(102, guid);

    var guidField = Assert.IsType<GuidFieldValue>(result);
    Assert.Equal(guid, guidField.Value);
  }

  [Fact]
  public void Map_ClrInt_ReturnsIntegerFieldValue()
  {
    var result = _mapper.Map(103, 42);

    var intField = Assert.IsType<IntegerFieldValue>(result);
    Assert.Equal(42, intField.Value);
  }

  [Fact]
  public void Map_ClrDecimal_ReturnsDecimalFieldValue()
  {
    var result = _mapper.Map(104, 99.95m);

    var decField = Assert.IsType<DecimalFieldValue>(result);
    Assert.Equal(99.95m, decField.Value);
  }

  [Fact]
  public void Map_ClrDateTime_ReturnsDateFieldValue()
  {
    var now = DateTime.UtcNow;
    var result = _mapper.Map(105, now);

    var dateField = Assert.IsType<DateFieldValue>(result);
    Assert.Equal(now, dateField.Value);
  }

  [Fact]
  public void Map_JsonElementString_ReturnsStringFieldValue()
  {
    using var doc = JsonDocument.Parse("\"test-string\"");
    var result = _mapper.Map(106, doc.RootElement);

    var stringField = Assert.IsType<StringFieldValue>(result);
    Assert.Equal("test-string", stringField.Value);
  }

  [Fact]
  public void Map_JsonElementGuidString_ReturnsGuidFieldValue()
  {
    var guid = Guid.NewGuid();
    using var doc = JsonDocument.Parse($"\"{guid}\"");
    var result = _mapper.Map(107, doc.RootElement);

    var guidField = Assert.IsType<GuidFieldValue>(result);
    Assert.Equal(guid, guidField.Value);
  }

  [Fact]
  public void Map_JsonElementDateString_ReturnsDateFieldValue()
  {
    using var doc = JsonDocument.Parse("\"2026-10-29T14:00:00Z\"");
    var result = _mapper.Map(108, doc.RootElement);

    var dateField = Assert.IsType<DateFieldValue>(result);
    Assert.NotNull(dateField.Value);
  }

  [Fact]
  public void Map_JsonElementInteger_ReturnsIntegerFieldValue()
  {
    using var doc = JsonDocument.Parse("12345");
    var result = _mapper.Map(109, doc.RootElement);

    var intField = Assert.IsType<IntegerFieldValue>(result);
    Assert.Equal(12345, intField.Value);
  }

  [Fact]
  public void Map_JsonElementDecimal_ReturnsDecimalFieldValue()
  {
    using var doc = JsonDocument.Parse("123.45");
    var result = _mapper.Map(110, doc.RootElement);

    var decField = Assert.IsType<DecimalFieldValue>(result);
    Assert.Equal(123.45m, decField.Value);
  }

  [Fact]
  public void Map_JsonElementIntList_ReturnsIntegerListFieldValue()
  {
    using var doc = JsonDocument.Parse("[1, 2, 3]");
    var result = _mapper.Map(111, doc.RootElement);

    var listField = Assert.IsType<IntegerListFieldValue>(result);
    Assert.Equal([1, 2, 3], listField.Value);
  }

  [Fact]
  public void Map_JsonElementStringList_ReturnsStringListFieldValue()
  {
    using var doc = JsonDocument.Parse("[\"a\", \"b\"]");
    var result = _mapper.Map(112, doc.RootElement);

    var listField = Assert.IsType<StringListFieldValue>(result);
    Assert.Equal(["a", "b"], listField.Value);
  }
}