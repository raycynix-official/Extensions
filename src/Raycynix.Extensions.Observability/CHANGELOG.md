# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Migrated observability composition to the standard `IMeterFactory`-based Metrics 3.0 pipeline.
- Migrated tracing composition to the shared standard `ActivitySource` from Tracing 3.0.
- Metrics export is now selected explicitly by the host.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

### Changed
- Removed mandatory Raycynix logging registration from `AddRaycynixObservability()`.
- Core observability now composes metrics, tracing, and operation context while leaving logger provider selection to the host application.

## 1.0.0
### Added
- Initial package release.
