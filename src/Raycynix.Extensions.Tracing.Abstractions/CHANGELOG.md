# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Added
- Added the stable `RaycynixTracing` activity source name, version, and shared instance.

### Removed
- Removed `ITracer` in favor of `System.Diagnostics.ActivitySource` and `Activity`.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

## 1.0.0
### Added
- Initial package release.
