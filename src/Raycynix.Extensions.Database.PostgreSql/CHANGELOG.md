# Changelog

## 3.1.0

### Changed

- PostgreSQL provider settings and model defaults are now isolated per registered context.
- Context-specific migrations history table and schema settings are applied to EF Core.

## 3.0.1

### Added

- Added optional `PostgreSqlOptions.DefaultSchema`, configuration binding, and validation; explicit entity mappings override it and migration history remains unchanged.
- Included the default schema in EF Core model cache keys.
- Documented schema mapping with `DatabaseSchemaAttribute` and `EntitySchema(...)`, including default-schema fallback.

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Renamed `PostgreSqlConfiguration` to `PostgreSqlOptions` and moved it to the `Options` namespace.
- Changed the provider section to `DatabaseOptions:PostgreSqlOptions`.
- Added fail-fast validation for pool sizes and command timeout.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics for PostgreSQL provider validation, connection-string source selection, and EF Core provider configuration.

## 2.1.0
### Added
- Added PostgreSQL provider registration with provider-specific validation.
- Added Npgsql connection-string composition.
- Added retry, command timeout, pooling, error detail, and migrations assembly support.

## 2.0.0
### Added
- Added PostgreSQL provider registration through `AddPostgreSql(...)`.
- Added provider-specific structured configuration validation.
- Added Npgsql connection-string composition.
- Added command timeout, retry, pooling, error detail, and migrations assembly support.

## 1.0.2
### Added
- Initial package release.
