# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

### Fixed

- Replaced fixed delays in retry and dead-letter tests with acknowledgement completion signals to avoid stopping the consumer before publication.

## 3.0.0
### Changed
- Retry delays and retry/dead-letter publication now observe host shutdown cancellation.
- `X-Processing-Error` now contains the exception type name instead of the potentially sensitive exception message.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics for RabbitMQ publish and inbound consumer flows.

## 1.0.1
### Added
- Initial package release.
