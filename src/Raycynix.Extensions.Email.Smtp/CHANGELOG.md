# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- SMTP MIME creation now streams attachment content and disposes owned streams with the MIME message.
- Renamed `SmtpConfiguration` to `SmtpOptions` and moved it to the `Raycynix.Extensions.Email.Smtp.Options` namespace.
- Changed the provider configuration section from `EmailConfiguration:SmtpConfiguration` to `EmailOptions:SmtpOptions`, following the shared options type-name convention.
- SMTP timeout must now be greater than zero.
- Explicit SMTP username and password values must be configured together.
- Updated SMTP registration callbacks to use `Action<SmtpOptions>`.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics for SMTP configuration validation, MIME creation counts, and delivery phases.

## 2.0.0
### Added
- Added SMTP email provider registration through `AddSmtp(...)`.
- Added SMTP configuration binding and validation.
- Added MailKit-based SMTP `IEmailSender` implementation for text, HTML, recipients, reply-to, headers, and attachments.
