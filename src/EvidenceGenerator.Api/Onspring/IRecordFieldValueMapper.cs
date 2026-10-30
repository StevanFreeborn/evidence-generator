using Onspring.API.SDK.Models;

namespace EvidenceGenerator.Api.Onspring;

internal interface IRecordFieldValueMapper
{
  RecordFieldValue Map(int fieldId, object? value);
}