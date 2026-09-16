# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Added
- Added `SecretOptions.ContinueOnProviderError` to control fallback behavior after provider failures.
- Added configuration binding for `SecretOptions` through the shared Raycynix Configuration pipeline.
- Added stable `SecretProviderNames` values for built-in provider ordering.
- Added validation for duplicate, empty, and unregistered provider names.

### Changed
- Secret resolution now continues through the configured provider chain after non-cancellation provider failures by default.
- Moved `SecretOptions` to `Raycynix.Extensions.Secrets.Options`.
- Changed `ProviderOrder` from CLR `Type` values to configuration-friendly provider names.
- Changed `AddRaycynixSecrets(...)` to require `IConfiguration`.
- Changed `ConfigurationSecretProvider` to use direct `IConfiguration` injection.
- Standardized secret key and cancellation validation across built-in providers.
- Fixed TeamCity provider lookup to read the process environment variable without the build-parameter `env.` prefix.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics for secret resolution attempts and provider-chain outcomes.

## 1.1.0
### Added
- Added configuration-backed secret resolution through `IConfiguration`.
- Added `SecretOptions` for customizing provider precedence.
- Added required-secret and diagnostic APIs for provider-aware resolution and explain output.
- Added tests covering configuration-provider resolution and provider precedence.

### Changed
- Updated the default provider chain so configuration values are checked before environment-based fallbacks.
- Updated package documentation to position `Raycynix.Extensions.Secrets` as a unified secret-resolution layer over configuration and CI-friendly environment providers.

## 1.0.1
### Added
- Added a console example that demonstrates composite secret resolution across environment, GitHub-style, and TeamCity-style variables.

### Changed
- Clarified example usage around provider ordering and missing-secret fallback behavior.
