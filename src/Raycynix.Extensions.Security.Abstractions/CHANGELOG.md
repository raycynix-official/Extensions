# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Authorization attributes now support both classes and methods.
- Permission and role attributes reject null, empty, and whitespace-only values.
- Multi-value authorization attributes require at least one value, trim entries, and remove case-insensitive duplicates.
- Subject type attributes reject undefined enum values.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

## 1.0.0
### Added
- Initial package release.
