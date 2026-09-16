# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Updated database metrics and tracing integration for the Database 3.0 options contracts.
- Migrated database instruments to standard `System.Diagnostics.Metrics` with named tags and explicit units.
- Migrated database spans to the shared standard `ActivitySource` with client activity kinds, tags, and success/error status codes.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics for observability setup and operation recording.

## 2.1.0
### Added
- Added optional database tracing and metrics observability integration for initialization, creation, migration, and model-building operations.

## 2.0.0
### Added
- Added optional database observability integration with tracing and metrics support.
- Added `AddObservability()` for Raycynix database builder pipelines.
- Added operation counters and duration histograms for initialization, creation, migration, and model-building operations.

### Changed
- Moved the concrete database observability implementation out of the core database package.
