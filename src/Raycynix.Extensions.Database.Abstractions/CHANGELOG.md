# Changelog

## 3.1.0

### Added

- Added `IDatabaseContextServices<TContext>` for context-specific options and model configuration.
- Added `IDatabaseInitializer<TContext>` for targeted initialization.
- Exposed context, options, and configuration-section identity through `IDatabaseBuilder`.

## 3.0.1

### Added

- Added `IDatabaseProviderModelConfigurator` for provider model defaults and their cache identity.
- Added `DatabaseSchemaAttribute` for declaring a configurator's table schema.

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Renamed `DatabaseConfiguration` to `DatabaseOptions`.
- Renamed `ConnectionConfiguration` to `ConnectionOptions`.
- Moved database settings to `Raycynix.Extensions.Database.Abstractions.Options`.
- Made connection properties settable for configuration callbacks.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.

## 2.1.0
### Added
- Added provider-specific validation contracts.
- Added shared database configuration, provider, builder, model assembly registry, configurator, and observability abstractions.

## 2.0.0
### Added
- Added shared database configuration contracts.
- Added provider, builder, model assembly registry, configurator, initialization, and observability contracts.
- Added provider-specific validation to `IDatabaseProviderRegistration`.

### Changed
- Moved common database contracts out of the core database package for lighter provider and feature integrations.

## 1.0.2
### Added
- Initial package release.
