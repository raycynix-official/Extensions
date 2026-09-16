# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

### Fixed

- Updated the Prometheus exporter in tests and the example to 1.18.0-beta.1 for compatibility with OpenTelemetry SDK 1.18.0.

## 3.0.0
### Added
- Added stable OpenTelemetry registration for the shared Raycynix meter and ASP.NET Core request instrumentation.

### Removed
- Removed the prometheus-net HTTP middleware and endpoint wrappers.
- Removed implicit coupling to the obsolete metrics configuration model.
- Kept the prerelease OpenTelemetry Prometheus exporter out of the stable package dependency graph; applications can opt into it explicitly.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

## 1.0.0
### Added
- Initial package release.
