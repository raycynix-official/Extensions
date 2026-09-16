# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Aligned the package with the net10.0 ASP.NET Core package line.
- Endpoint declarations, MVC contract attributes, endpoint metadata, and contract HTTP results now reject invalid contract identities immediately.
- Incoming contract names are trimmed after successful header parsing.
- ModelState conversion now rejects empty top-level error codes and messages.
- Updated package description, tags, release notes, and README for the current ASP.NET Core integration.

## 2.2.0
### Added
- Started unified versioning for Raycynix packages from this release.
- Added optional `Microsoft.Extensions.Logging.ILogger<T>` diagnostics for contract metadata middleware and contract HTTP result execution.

## 1.0.0
### Added
- Initial package release.
