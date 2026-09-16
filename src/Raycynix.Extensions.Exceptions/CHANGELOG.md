# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Sensitive exception data now masks application-specific keys containing known secret-name fragments.
- Retry backoff is capped by `RetryExecutionOptions.MaxDelay` to prevent delay overflow.
- `ExceptionMapperOptions.Mappings` now exposes a read-only dictionary and rejects invalid mappings.
- Exception mappers validate null input and snapshot configured mappings.
- `ValidationException` snapshots validation errors instead of retaining caller-owned collections.
- Retry execution validates operation delegates, uses `Random.Shared`, and restores the previous execution context after completion.
- Background task execution validates operation delegates and operation names.
- Core DI registration now validates the service collection and preserves user-provided service implementations.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics without requiring Raycynix logging abstractions.

## 1.0.0
### Added
- Initial package release.
