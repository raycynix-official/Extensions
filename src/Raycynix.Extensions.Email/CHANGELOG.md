# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Renamed `EmailConfiguration` to `EmailOptions` and moved it to the `Raycynix.Extensions.Email.Options` namespace.
- Changed the default configuration section from `EmailConfiguration` to `EmailOptions`, following the shared options type-name convention.
- Default sender and reply-to display names now require their corresponding addresses.
- Updated registration callbacks to use `Action<EmailOptions>`.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics for email provider resolution.

## 2.0.0
### Added
- Added shared Raycynix email registration infrastructure.
- Added `AddRaycynixEmail(...)` service registration API.
- Added shared email configuration for default sender and reply-to addresses.
- Added email provider descriptor resolution.
