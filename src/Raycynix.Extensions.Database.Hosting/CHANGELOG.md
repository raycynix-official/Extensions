# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Updated generic-host initialization for the Database 3.0 options contracts.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics for startup database initialization.

## 2.1.0
### Added
- Added generic host and `IServiceProvider` startup helpers for running Raycynix database initialization through `IDatabaseInitializer`.

## 2.0.0
### Added
- Added generic host and service provider startup helpers for running `IDatabaseInitializer`.

## 1.0.2
### Added
- Initial package release.
