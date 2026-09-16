# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Replaced the custom Prometheus-backed metrics service with standard `System.Diagnostics.Metrics` and `IMeterFactory` registration.
- Removed configuration, health-check registration, global Prometheus registry ownership, and metric wrapper implementations from the core package.
- Made exporters and endpoint exposure explicit host concerns.

### Removed
- Removed `MetricsConfiguration` and its ineffective `UsePrometheus`, `UseHealthChecks`, and `MetricsEndpoint` settings.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging` diagnostics for metrics configuration validation and metric creation.

## 1.0.1
### Added
- Initial package release.
