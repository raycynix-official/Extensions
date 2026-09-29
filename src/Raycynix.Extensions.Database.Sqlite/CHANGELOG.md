# Changelog

## 3.1.0

### Changed

- SQLite provider settings are now isolated per registered context.
- Context-specific migrations history table settings are applied to EF Core.

## 3.0.1

### Added

- Documented and tested omission of explicit schema names from SQLite queries and table-creation SQL.

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Renamed `SqliteConfiguration` to `SqliteOptions` and moved it to the `Options` namespace.
- Changed the provider section to `DatabaseOptions:SqliteOptions`.
- Added fail-fast validation for mode, cache, and command timeout.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics for SQLite provider validation, connection-string source selection, and EF Core provider configuration.

## 2.1.0
### Added
- Added SQLite provider registration with provider-specific validation.
- Added SQLite connection-string composition.
- Added mode, cache, command timeout, and migrations assembly support.

## 2.0.0
### Added
- Added SQLite provider registration through `AddSqlite(...)`.
- Added provider-specific structured configuration validation.
- Added SQLite connection-string composition.
- Added mode, cache, command timeout, and migrations assembly support.

## 1.0.2
### Added
- Initial package release.
