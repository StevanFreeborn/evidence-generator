# Evidence Generator

Evidence Generator is an asynchronous .NET web service that generates files and attaches them to records in [Onspring](https://onspring.com/).

Requests submitted to `POST /generate` are validated and queued for background processing, where evidence files are generated in memory and uploaded to Onspring via the official SDK.

## Documentation

The service includes a built-in documentation webpage that serves as the default landing page. When running the service, navigate to the root URL in your browser to view:

- Complete schema specifications and required fields
- Format-specific request examples for all supported evidence types
- An LLM prompt helper designed for easy copy-pasting to generate valid payloads
- Status code definitions and troubleshooting guidance

## Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Common Commands

```bash
# Build solution
dotnet build

# Run test suite
dotnet test

# Run the API locally
dotnet run --project src/EvidenceGenerator.Api

# Publish container image
dotnet publish src/EvidenceGenerator.Api/EvidenceGenerator.Api.csproj -t:PublishContainer /p:PublishContainer=true
```

## Configuration

Configuration uses standard ASP.NET Core mechanisms. Settings can be defined in `appsettings.json` (see `appsettings.Example.json` for reference) or supplied using environment variables:

- `Seq__ServerUrl`: URL of the Seq instance for OpenTelemetry logs and traces.
- `Seq__ApiKey`: API key for authenticated Seq ingestion (optional).
- `Onspring__BaseAddress`: Target Onspring API address (defaults to `https://api.onspring.com/`).
- `Logging__LogLevel__*`: Logging level overrides.

## License

This project is licensed under the [MIT License](LICENSE.md) &bull; Copyright &copy; 2026 Stevan Freeborn.
