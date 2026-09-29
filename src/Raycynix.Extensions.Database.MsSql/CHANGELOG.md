# Changelog

## 3.1.0

### Changed

- SQL Server provider settings and model defaults are now isolated per registered context.
- Context-specific migrations history table and schema settings are applied to EF Core.

## 3.0.1

### Added

- Added optional DefaultSchema with configuration binding, validation, mapping precedence, and runtime/design-time model-cache isolation. Migration history remains unchanged.

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Renamed `MsSqlServerConfiguration` to `MsSqlServerOptions` and moved it to the `Options` namespace.
- Changed the provider section to `DatabaseOptions:MsSqlServerOptions`.
- Added fail-fast validation for command timeout.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics for SQL Server provider validation, connection-string source selection, and EF Core provider configuration.

## 2.1.0
### Added
- Added SQL Server provider registration with provider-specific validation.
- Added SQL Server connection-string composition.
- Added retry, command timeout, MARS, certificate trust, and migrations assembly support.

### Changed
- Kept `TrustServerCertificate` disabled by default unless explicitly configured.

## 2.0.0
### Added
- Added SQL Server provider registration through `AddMsSql(...)`.
- Added provider-specific structured configuration validation.
- Added SQL Server connection-string composition.
- Added command timeout, retry, MARS, certificate trust, and migrations assembly support.

### Changed
- Default `TrustServerCertificate` is disabled unless explicitly configured.

## 1.0.2
### Added
- Initial package release.
