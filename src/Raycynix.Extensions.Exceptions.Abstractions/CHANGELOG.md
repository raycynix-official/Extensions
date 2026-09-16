# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Added `RetryExecutionOptions.MaxDelay` and bounded retry option validation.
- Validated exception messages, error codes, error categories, HTTP status codes, and exception detail values.
- Exception details are copied when a `RaycynixException` is created.
- Changed validation error response contracts to expose `IReadOnlyDictionary<string, string[]>`.
- Removed raw query strings and internal execution context from `IExceptionResponse`.
- Changed `ExceptionDetail` to a validated sealed record.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

## 1.0.0
### Added
- Initial package release.
