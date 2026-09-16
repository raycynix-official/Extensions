# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Removed raw query strings and internal execution context from public JSON error responses.
- Client-request cancellation is propagated instead of being swallowed by the middleware.
- `IOperationContext` integration is optional; correlation metadata is included when the service is registered.
- Error response writes now observe `HttpContext.RequestAborted`.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics in the exception middleware.
### Changed
- Middleware logging no longer writes raw query string values.

## 1.0.0
### Added
- Initial package release.
