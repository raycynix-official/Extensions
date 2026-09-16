# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

### Fixed

- Aligned the Prometheus exporter used by tests and the example with OpenTelemetry SDK 1.18.0, fixing metrics endpoint HTTP 500 failures.

## 3.0.0
### Changed
- Incoming correlation identifiers are now single-value, length-bounded, and restricted to a safe ASCII character set before logging or propagation.
- Added OpenTelemetry ASP.NET Core metrics instrumentation through Metrics 3.0.
- Added OpenTelemetry ASP.NET Core tracing instrumentation through Tracing 3.0.
- `MapRaycynixObservabilityEndpoints` now maps health checks only; metrics exporters expose their endpoints explicitly.
- Removed the obsolete metrics request middleware from `UseRaycynixObservability`.
- Removed the obsolete custom tracing middleware from `UseRaycynixObservability`.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added provider-agnostic correlation diagnostics through standard `Microsoft.Extensions.Logging` scopes.
- Added `ObservabilityAspNetCoreConfiguration.IncludeIdentityInLoggingScope` for disabling user and subject values in request logging scopes.

### Changed
- Removed Serilog-specific log context usage from correlation middleware.

## 1.0.0
### Added
- Initial package release.
