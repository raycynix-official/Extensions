# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Renamed `SecurityConfiguration` to `SecurityOptions` and moved it to `Raycynix.Extensions.Security.Options`.
- Renamed `JwtConfiguration` to `JwtOptions` and moved it to `Raycynix.Extensions.Security.Options`.
- Changed the root configuration section from `SecurityConfiguration` to `SecurityOptions`.
- Registered `JwtOptions` directly from the bound `SecurityOptions.JwtOptions` snapshot, fixing empty JWT settings when the nested type is injected.
- JWT authority values must be absolute HTTP or HTTPS URIs and must use HTTPS when metadata HTTPS is required.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

## 1.0.1
### Added
- Added a console example that demonstrates configuration binding for `SecurityConfiguration`.

### Changed
- Clarified package boundaries by keeping the core example focused on configuration and host-agnostic registration only.
