# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Added
- Added configurable global and named ASP.NET Core rate limit policies.
- Added fixed-window, sliding-window, token-bucket, and concurrency algorithms with IP, JWT subject, or global partitioning.
- Added consistent rate limit rejection responses and optional `Retry-After` headers.

### Changed
- Incomplete authenticated principals now fail closed as anonymous Raycynix security contexts instead of causing request-time exceptions.
- Subject-based rate limiting now trusts subject claims only from authenticated principals.
- Replaced the separate manual JWT binding path with the shared core `SecurityOptions` pipeline.
- JWT bearer options, `SecurityOptions`, and directly injected `JwtOptions` now use the same bound snapshot.
- Missing JWT authority is reported through options validation when security options are resolved.
- Dynamic policy helpers and authorization requirements now reject empty values and snapshot normalized collections.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics for JWT challenges, security context mapping, dynamic policy resolution, authorization handlers, and authorization failure responses.

## 1.0.0
### Added
- Added an ASP.NET Core example that demonstrates JWT security registration, request pipeline wiring, and protected endpoints.
- Added example endpoints that show `RequireRaycynixAuthorization(...)`, dynamic permission policies, and request-scoped `ISecurityContext` usage.

### Changed
- Added example configuration for JWT authority, issuer, audience, and HTTPS metadata handling.
