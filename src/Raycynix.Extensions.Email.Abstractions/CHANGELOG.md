# Changelog

## 3.0.1

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- In-memory attachment factories now create a single defensive content copy.
- Rejects line breaks in subjects, attachment identifiers, file names, and custom header values.
- Validates custom header names and send-result metadata before provider execution.
- Rejects whitespace-only optional HTML fallback content, message identifiers, and provider error codes.
- Attachment factories now honor cancellation before opening or allocating content streams.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.

## 2.0.0
### Added
- Added email sender, provider registration, and builder contracts.
- Added email message, address, body, attachment, and send result models.
- Added email send and provider configuration exceptions.
