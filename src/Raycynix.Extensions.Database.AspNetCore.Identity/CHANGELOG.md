# Changelog

## 3.1.0

### Added

- Added support for registering multiple independently configured Identity context types.
- Added configuration-section overloads for Identity context registration.

### Changed

- Assemblies registered before an Identity context are now preserved when its isolated model registry is created.

## 3.0.1

### Added

- Verified default-schema inheritance for all Identity tables with PostgreSQL and SQL Server, including registration without schema configuration.

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Updated default, generic, and custom Identity contexts to use `DatabaseOptions`.
- Updated configuration examples to the `DatabaseOptions` and provider-specific options section names.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.

## 2.1.0

- Added ASP.NET Core Identity database integration for `Raycynix.Extensions.Database`.
- Added the default `RaycynixIdentityDatabaseContext`.
- Added generic Raycynix Identity contexts for custom users, roles, keys, claims, logins, and tokens.
- Added `IRaycynixIdentityDatabaseContext` so identity registration remains scoped to Identity-compatible database contexts.
- Added `AddRaycynixIdentityDatabase` registration overloads for caller, marker, explicit model, and explicit migrations assembly scenarios.
- Integrated Raycynix model configurators, provider validation, initialization, and model-cache behavior with ASP.NET Core Identity contexts.
