# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Aligned the package version with the net10.0 ASP.NET Core package line.
- Updated ASP.NET Core configuration setup APIs to use `ConfigurationSourcesOptions` naming through the core configuration package.
- Configuration source setup now resolves appsettings and dotenv files from the application content root.
- Updated package description, tags, release notes, and README content for feature gate options, middleware diagnostics, endpoint metadata, and Minimal API feature gates.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional feature gate middleware diagnostics through standard `Microsoft.Extensions.Logging.ILogger<T>`.
- Added `LogDebug` diagnostics for feature gate evaluation flow.

## 2.0.0
### Added
- Added configurable feature gate response status codes.
- Added lazy feature flag accessor resolution so ungated endpoints do not require feature flag services.
- Added feature gate options registration helper.
- Aligned ASP.NET Core integrations with Configuration v2.

## 1.0.0
### Added
- Initial package release.
