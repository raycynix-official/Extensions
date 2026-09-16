# Changelog

## 3.0.1

### Added

- Documented and tested explicit schema mappings as database-qualified table names in the Oracle MySql.EntityFrameworkCore provider.

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Renamed `MySqlConfiguration` to `MySqlOptions` and moved it to the `Options` namespace.
- Changed the provider section to `DatabaseOptions:MySqlOptions`.
- Added fail-fast validation for command timeout.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics for MySQL provider validation, connection-string source selection, and EF Core provider configuration.

## 2.1.0
### Added
- Added MySQL provider registration with provider-specific validation.
- Added MySQL connection-string composition.
- Added retry, command timeout, pooling, user-variable, and migrations assembly support.

## 2.0.0
### Added
- Added MySQL provider registration through `AddMySql(...)`.
- Added provider-specific structured configuration validation.
- Added MySQL connection-string composition.
- Added command timeout, retry, pooling, user-variable, and migrations assembly support.

## 1.0.2
### Added
- Initial package release.
