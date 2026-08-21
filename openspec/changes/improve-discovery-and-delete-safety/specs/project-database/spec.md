## ADDED Requirements

### Requirement: Private Linux project database artifacts
When `dbox init` runs on Linux, the system SHALL set `.dbox` to POSIX mode
`0700` and `data.db` to POSIX mode `0600`. It SHALL also set any SQLite
sidecar artifacts created or found for that database, including `-wal`, `-shm`,
and `-journal` files, to POSIX mode `0600`. On non-Linux systems, the system
SHALL initialize the project without claiming or reporting POSIX mode values.

#### Scenario: Initialize a new Linux project privately
- **WHEN** `dbox init` creates a project database on Linux
- **THEN** `.dbox` has mode `0700` and `data.db` has mode `0600` after initialization

#### Scenario: Reinitialize an existing Linux project privately
- **WHEN** `dbox init` runs on Linux against an existing project database or SQLite sidecar artifact with broader permissions
- **THEN** it preserves the database contents while setting `.dbox` to `0700` and the database and sidecar artifacts to `0600`

#### Scenario: Initialize on a non-Linux system
- **WHEN** `dbox init` runs on a non-Linux system
- **THEN** it completes the documented initialization behavior without returning or promising POSIX permission modes

## MODIFIED Requirements

### Requirement: Nearest project database discovery
Every command that requires a project database SHALL resolve the first `.dbox`
directory found while walking from the current directory toward the filesystem
root and SHALL use its `data.db` file. `dbox context` SHALL perform the same
walk to report its result without requiring the file to exist. `dbox activity
schema` SHALL not perform this discovery.

#### Scenario: Discover a database in a parent
- **WHEN** a database-backed command runs from a descendant directory with no local `.dbox` and an ancestor contains `.dbox/data.db`
- **THEN** the command uses the ancestor database

#### Scenario: Prefer the nearest database
- **WHEN** both a descendant and an ancestor contain `.dbox/data.db`
- **THEN** a database-backed command uses the descendant database

#### Scenario: Stop at an incomplete project boundary
- **WHEN** the first `.dbox` directory found while walking upward does not contain `data.db`
- **THEN** database-backed discovery stops at that directory and does not search for a database in higher ancestors

#### Scenario: No project database exists
- **WHEN** no valid `.dbox/data.db` is found before reaching the filesystem root
- **THEN** a database-backed command stops before its main operation and returns the database-not-found result defined by the CLI contract

### Requirement: Automatic migration before data operations
Every catalog command that resolves a project database to execute a data
operation SHALL apply pending known migrations before its main operation. This
includes every persistent `dbox activity` data command, but excludes `dbox
activity schema` and `dbox activity delete --dry-run`.

#### Scenario: Run a command against a database with pending migrations
- **WHEN** a catalog data operation that is not a dry run resolves a database that has pending known migrations
- **THEN** the migrations are applied before the requested operation reads or changes data

#### Scenario: Migration fails
- **WHEN** a required migration cannot be applied
- **THEN** the command returns a database error, does not execute the requested operation, and does not delete or replace the database
