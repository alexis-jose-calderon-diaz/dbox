## MODIFIED Requirements

### Requirement: Project-local initialization

The system SHALL initialize only the current working directory when the user runs `dbox init`. This root command is shared infrastructure for every current and future catalog and SHALL apply all migrations known to the installed CLI.

#### Scenario: Initialize a new directory

- **WHEN** `dbox init` runs in a directory without `.dbox/data.db`
- **THEN** the system creates `.dbox`, creates `.dbox/data.db`, applies all known migrations, and reports an initialized database

#### Scenario: Repeat an up-to-date initialization

- **WHEN** `dbox init` runs against an already initialized database with no pending migrations
- **THEN** the system preserves all existing data and reports that the database is already initialized

#### Scenario: Migrate an existing database

- **WHEN** `dbox init` finds an existing database with pending known migrations
- **THEN** the system applies the migrations without deleting or replacing the database and reports that it was migrated

#### Scenario: Initialize a nested project

- **WHEN** `dbox init` runs from a subdirectory whose parent already contains a dbox database
- **THEN** the system creates and uses a new `.dbox/data.db` in the current directory without modifying the parent database

### Requirement: Automatic migration before data operations

Every catalog command that resolves a project database SHALL apply pending known migrations before its main operation, including `dbox activity schema` and every `dbox activity` data command.

#### Scenario: Run a command against a database with pending migrations

- **WHEN** a catalog command resolves a database that has pending known migrations
- **THEN** the migrations are applied before the requested operation reads or changes data

#### Scenario: Migration fails

- **WHEN** a required migration cannot be applied
- **THEN** the command returns a database error, does not execute the requested operation, and does not delete or replace the database
