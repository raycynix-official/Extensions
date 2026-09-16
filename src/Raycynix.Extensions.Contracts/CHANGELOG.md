# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Aligned the package with the net10.0 package line.
- `ContractVersion.Initial` now returns an independent `1.0.0` instance so callers cannot mutate shared global state.
- Contract evolution attributes now parse and normalize semantic versions when constructed.
- `PageInfo.IsValid()` now verifies that navigation flags match the current page and total page count.
- Updated package description, tags, release notes, and README for the current contract API.

### Fixed
- Contract model `IsValid()` methods now safely reject malformed deserialized null members instead of throwing.
- `ContractMetadata.HasIdentity` and `ToString()` now safely handle a missing version.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.

## 1.0.0
### Added
- Initial package release.
