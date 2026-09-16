# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Aligned the package version with the net10.0 / Microsoft.Extensions 10 package line.
- Updated package description, tags, release notes, and README content for the current typed options, diagnostics, redaction, feature flag, validation, reload, and change-handler contracts.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Documented the standard optional Microsoft.Extensions logging approach for runtime diagnostics implemented by concrete packages.

## 2.0.0
### Added
- Added configuration diagnostics contracts.
- Added redaction contract for diagnostics snapshots.
- Added registration and reload diagnostic models.
- Added expanded reload results for apply, reject, ignore, and restart-required decisions.
- Added named-options and required-section metadata support.

## 1.0.0
### Added
- Initial package release.
