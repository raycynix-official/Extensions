# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Aligned the package with the net10.0 Raycynix 3.0 package line.
- Existing operation-context, assembly helper, and disposable APIs remain compatible.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

### Changed
- Fallback `OperationContext.TraceId` values are now stable within the same context instance when no `Activity.Current` exists.

## 1.0.1
### Added
- Initial package release.
