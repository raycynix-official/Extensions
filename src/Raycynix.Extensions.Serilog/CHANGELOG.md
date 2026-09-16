# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0

### Added

* Added the new `Raycynix.Extensions.Serilog` package.
* Added `AddRaycynixSerilog()` for `IHostApplicationBuilder`.
* Added support for `HostApplicationBuilder`.
* Added support for `WebApplicationBuilder` through `IHostApplicationBuilder`.
* Added `UseRaycynixSerilog()` for the classic `IHostBuilder`.
* Added direct `IServiceCollection` registration for custom hosting models and integration tests.
* Added native `Microsoft.Extensions.Logging` integration through the official Serilog provider.
* Added support for standard `ILogger<T>`.
* Added support for `ILoggerFactory` and dynamically created logger categories.
* Added native Serilog configuration through `ReadFrom.Configuration()`.
* Added dependency-injected Serilog components through `ReadFrom.Services()`.
* Added the `Raycynix:Serilog` configuration section.
* Added automatic service name resolution from `IHostEnvironment.ApplicationName`.
* Added automatic service version resolution from the entry assembly.
* Added automatic environment resolution from `IHostEnvironment.EnvironmentName`.
* Added standard `ServiceName`, `ServiceVersion`, and `Environment` event properties.
* Added `Enrich.FromLogContext()` by default.
* Added default `Microsoft` and `System` minimum-level overrides.
* Added an optional fallback console sink when no native Serilog sink is configured.
* Added configurable fallback console output template.
* Added support for custom native Serilog configuration section names.
* Added `RaycynixSerilogBuilder`.
* Added `RaycynixSerilogContext`.
* Added `IRaycynixSerilogConfigurator`.
* Added typed configurator registration.
* Added existing configurator instance registration.
* Added ordered inline Serilog configurators.
* Added duplicate-registration detection.
* Added `PreserveStaticLogger` configuration.
* Added `WriteToProviders` configuration.
* Added extension points for optional integration packages such as `Raycynix.Extensions.Serilog.Elastic`.
* Added explicit sink configurators and `ConfigureSink()` for fallback-console detection.
* Added the optional `Raycynix.Extensions.Serilog.Aspire` AppHost integration.
* Added complete package-family, options, extension API, reload, and troubleshooting documentation.

### Changed

* Renamed the package from `Raycynix.Extensions.Logging` to `Raycynix.Extensions.Serilog`.
* Changed the package purpose from a custom logging abstraction to a thin Serilog integration and convention layer.
* Consolidated host and service registration into one registration operation.
* Moved the actual registration pipeline into a shared internal implementation used by all supported builder types.
* Changed application logging to use `Microsoft.Extensions.Logging.ILogger<T>`.
* Changed structured logging to rely on the official Serilog Microsoft logging provider.
* Changed scopes to use the standard `Microsoft.Extensions.Logging` scope implementation.
* Changed sink, filter, level, formatter, and destructuring configuration to use native Serilog configuration.
* Changed correlation and request metadata enrichment to rely on standard logging scopes and observability integrations.
* Changed optional sink integrations to separate packages.
* Reduced the core package dependency set to host integration, native Serilog configuration, and console fallback support.
* Changed fallback console detection to account for native, dependency-injected, inline, and integration-provided sinks.

### Removed

* Removed `Raycynix.Extensions.Logging.Abstractions.ILogger<T>`.
* Removed the custom Raycynix logger implementation.
* Removed the custom Microsoft logging to Serilog adapter.
* Removed custom message-template forwarding.
* Removed custom `EventId` handling.
* Removed custom `LogLevel` mapping.
* Removed custom scope handling.
* Removed direct per-log-event correlation enrichment.
* Removed the mandatory split between `UseRaycynixLogging()` and `AddRaycynixLogging()`.
* Removed `LoggingConfiguration.MinimumLevel`.
* Removed `LoggingConfiguration.OutputTemplate`.
* Removed sink-specific settings from the core configuration model.
* Removed Elasticsearch dependencies from the core package.
* Removed the dependency on `Raycynix.Extensions.Configuration`.
* Removed the separate `Raycynix.Extensions.Logging.Abstractions` package from the new architecture.

## 2.2.0

### Added

- Starts unified versioning for Raycynix packages from this release.

## 2.1.0

### Added

- Added `LoggingBuilder` for optional logging integrations.
- Added support for external Serilog configurators through `IRaycynixLoggingConfigurator`.
- Added a no-configuration `AddRaycynixLogging()` overload for default logging registration.

### Changed

- Moved shared logging configuration types to `Raycynix.Extensions.Logging.Abstractions`.
- Removed Elasticsearch sink setup from the base logging package; use `Raycynix.Extensions.Logging.Elastic` when
  Elasticsearch output is required.
- `UseRaycynixLogging(...)` now prefers the `LoggingConfiguration` registered in DI, while preserving the previous
  fallback to host configuration and defaults when logging services are not registered.

## 2.0.0

### Added

- Added updated package examples and test coverage for the structured logging API.

### Changed

- Reworked the logger API around message-template arguments instead of metadata payload objects.
- Updated typed logger overloads and XML documentation to reflect the new template-based contract.

### Fixed

- Fixed correlation id enrichment so ambient operation context values are included in log events.
- Fixed log output when no extra structured arguments are provided so messages no longer append `null`.
