# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Added
- Added a built-in dotenv configuration provider through `AddEnvFile(...)`.
- Added optional `.env` loading to the standard configuration source set.
- Added support for comments, `export` declarations, quoted values, inline comments, and `__` section delimiters in dotenv files.
- Added `ConfigurationSectionPath` for deriving root and nested configuration paths from options type names.

### Changed
- Configuration change handlers are now invoked sequentially in reload order and are cancelled and awaited during host shutdown.
- Aligned the package version with the net10.0 / Microsoft.Extensions 10 package line.
- Renamed the source setup options type from `ConfigurationSourcesConfiguration` to `ConfigurationSourcesOptions` to follow the Options naming convention for setup objects.
- Standard typed configuration registration now resolves its default section through the shared options type-name convention.
- Process environment variables and command-line arguments retain higher priority than dotenv values.
- Updated package description, tags, release notes, and README content for the current typed options, diagnostics, feature flag, reload, and logging behavior.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics that do not require a logger provider to be registered.
- Added detailed `LogDebug` diagnostics for configuration change tracking startup, registrations, reload policy decisions, and change handler execution.
- Added optional diagnostics for configuration validation and redacted snapshot access.

## 2.0.0
### Added
- Added required-section validation for typed configuration and feature flags.
- Added named options support for typed registrations and accessors.
- Added approved runtime snapshots so rejected reloads do not replace the exposed current configuration.
- Added diagnostics for registrations, retained reload decisions, and redacted snapshots.
- Added configurable diagnostics options for snapshot access and reload history length.
- Added default and custom configuration redactor support.
- Added expanded reload behaviors for apply, reject, ignore, and restart-required decisions.

## 1.0.1
### Added
- Initial package release.
