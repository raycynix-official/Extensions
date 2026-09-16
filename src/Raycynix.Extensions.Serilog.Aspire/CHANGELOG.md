# Changelog

## 3.0.1

### Changed

- Updated `Aspire.Hosting` to 13.5.4.
- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0

### Added

- Added `AddRaycynixSerilog()` for `IDistributedApplicationBuilder`.
- Added direct Serilog registration for the Aspire AppHost process.
- Documented the separate logging lifecycle of orchestrated resources.
- Added support for the core options callback and optional sink integrations.
- Added AppHost configuration, ServiceDefaults, and resource-boundary guidance.
