# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0

### Added

- Added integration with the officially supported `Elastic.Serilog.Sinks`.
- Added direct Elasticsearch node connections.
- Added optional Elasticsearch node sniffing.
- Added Elastic Cloud support.
- Added API key authentication.
- Added basic authentication.
- Added ECS data stream configuration.
- Added ILM policy configuration.
- Added sink-specific minimum level.
- Added ECS host, process, user, and Activity configuration.
- Added channel buffer configuration.
- Added proxy configuration.
- Added certificate fingerprint configuration.
- Added transport and sink escape hatches.
- Added startup configuration validation.

### Changed

- Renamed the package from  `Raycynix.Extensions.Logging.Elastic` to `Raycynix.Extensions.Serilog.Elastic`.
- Removed the dependency on the old Raycynix logging abstraction.
- Moved all sink-specific dependencies out of the core Serilog package.

### Fixed

- Prevented the Elasticsearch configurator from resolving options through the logging provider during Serilog startup.
- Aligned transitive Elastic ingest and transport dependencies with `Elastic.Serilog.Sinks` 9.0.0 to prevent runtime type-load failures.
- Prevented the fallback console sink from being added when the enabled Elastic integration contributes output.

## 2.2.0

### Added

- Starts unified versioning for Raycynix packages from this release.

## 2.1.0

### Added

- Initial Elasticsearch logging integration package.
- Added `AddElastic()` for registering the Serilog Elasticsearch sink through `LoggingBuilder`.
- Added `ElasticConfiguration`, bound from `LoggingConfiguration:ElasticConfiguration`.
