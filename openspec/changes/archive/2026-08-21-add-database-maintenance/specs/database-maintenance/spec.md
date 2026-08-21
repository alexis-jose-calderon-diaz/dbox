## Purpose

Provide project-local database preservation and non-invasive health diagnostics
for dbox SQLite databases without exposing catalog data access or repair tools.

## ADDED Requirements

### Requirement: Create a consistent project database backup
The system SHALL expose `dbox backup` as a root maintenance command. It SHALL
resolve the nearest project database using the standard discovery rules and
create an online consistent copy without modifying the source database. The
copy SHALL be created in the resolved project's `.dbox/backups` directory with
a filename containing a UTC timestamp and a `.db` extension.

#### Scenario: Back up a resolved project database
- **WHEN** a user runs `dbox backup` from a directory that resolves a project database
- **THEN** the system creates a consistent SQLite backup in that project's `.dbox/backups` directory and writes exactly one JSON result identifying the backup

#### Scenario: Back up from a descendant directory
- **WHEN** a user runs `dbox backup` from a descendant of a project directory
- **THEN** the system backs up the nearest resolved project's database rather than creating or using a database in the current directory

#### Scenario: Fail to back up a missing project database
- **WHEN** a user runs `dbox backup` without a valid resolved project database
- **THEN** the system returns the database-not-found result defined by the CLI contract and creates no backup

#### Scenario: Fail to create a backup
- **WHEN** the system cannot create the backup directory or copy the resolved database
- **THEN** the system returns the database error defined by the CLI contract and does not report a successful backup

### Requirement: Diagnose a project database without modification
The system SHALL expose `dbox doctor` as a root maintenance command. It SHALL
resolve the nearest project database using the standard discovery rules and
report whether the database exists, can be opened, passes SQLite integrity
checking, has pending known migrations, and has the inspected access
permissions. The command SHALL be strictly read-only: it SHALL NOT apply
migrations, repair the database, create a database, or modify files.

#### Scenario: Diagnose a healthy up-to-date database
- **WHEN** a user runs `dbox doctor` against a readable, intact database with no pending known migrations
- **THEN** stdout contains exactly one JSON diagnostic result reporting existence, successful opening, passing integrity, no pending migrations, and the inspected permissions

#### Scenario: Report pending migrations without applying them
- **WHEN** a user runs `dbox doctor` against a database with pending known migrations
- **THEN** the diagnostic result reports the pending migrations and the database migration history remains unchanged

#### Scenario: Report an unreadable or corrupt database
- **WHEN** a user runs `dbox doctor` against a database that cannot be opened or does not pass SQLite integrity checking
- **THEN** the command reports the failed diagnostic status without attempting to repair, migrate, replace, or otherwise modify the database

#### Scenario: Fail to diagnose a missing project database
- **WHEN** a user runs `dbox doctor` without a valid resolved project database
- **THEN** the system returns the database-not-found result defined by the CLI contract and does not create project files
