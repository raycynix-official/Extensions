# Changelog

## 3.1.0

### Changed

- Messaging database startup initialization now cooperates with applications that register multiple database contexts.

## 3.0.1

### Added

- Verified Inbox/Outbox default-schema inheritance and generated DDL with PostgreSQL and SQL Server.

### Changed

- Inherit the shared package version from the repository-root `Directory.Build.props`; package-local version declarations have been removed.

## 3.0.0
### Changed
- Moved outbox availability, retention, ordering, and batch limits into provider-translated SQL queries.
- Persisted messaging timestamps as UTC ticks to provide consistent comparisons and ordering across SQLite, PostgreSQL, SQL Server, and MySQL.
- Added status/timestamp indexes for inbox and outbox retention cleanup queries.

### Fixed
- Detached failed concurrent inbox inserts before loading and reclaiming the winning row.

### Migration
- Existing inbox and outbox timestamp columns must be converted to signed 64-bit UTC tick values when upgrading from 2.x.

## 2.2.0
### Added
- Starts unified versioning for Raycynix packages from this release.
- Added optional Microsoft `ILogger<T>` diagnostics for inbox/outbox persistence, leases, and cleanup cycles.

## 1.0.1
### Added
- Added package-level changelog tracking.
- Expanded persistence test coverage around inbox/outbox concurrency, stale lease recovery, and cleanup behavior.

### Changed
- Updated test infrastructure to align with the current logging abstractions used by the shared packages.

### Fixed
- Updated no-op test logger implementations so database-backed messaging tests are no longer sensitive to removed metadata logging calls.
