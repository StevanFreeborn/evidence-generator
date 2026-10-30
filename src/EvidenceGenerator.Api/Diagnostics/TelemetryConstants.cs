namespace EvidenceGenerator.Api.Diagnostics;

internal static class TelemetryConstants
{
  public const string ServiceName = "EvidenceGenerator.Api";
  public const string ActivitySourceName = "EvidenceGenerator.Api";

  internal static class Activities
  {
    public const string ProcessEvidenceJob = "ProcessEvidenceJob";
  }

  internal static class Tags
  {
    public const string JobId = "job.id";
    public const string AppId = "app.id";
    public const string RecordId = "record.id";
    public const string FieldId = "field.id";
    public const string FileType = "file.type";
    public const string FileName = "file.name";
    public const string FileSizeBytes = "file.size_bytes";
  }
}