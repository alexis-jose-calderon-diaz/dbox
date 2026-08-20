# project-database Specification

## Purpose

Define the local, isolated SQLite lifecycle that lets every dbox project own and discover its database without global state.

## Requirements

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

### Requirement: Nearest project database discovery

Every command except `init` SHALL resolve the first `.dbox` directory found while walking from the current directory toward the filesystem root and SHALL use its `data.db` file.

#### Scenario: Discover a database in a parent

- **WHEN** a data command runs from a descendant directory with no local `.dbox` and an ancestor contains `.dbox/data.db`
- **THEN** the command uses the ancestor database

#### Scenario: Prefer the nearest database

- **WHEN** both a descendant and an ancestor contain `.dbox/data.db`
- **THEN** the command uses the descendant database

#### Scenario: Stop at an incomplete project boundary

- **WHEN** the first `.dbox` directory found while walking upward does not contain `data.db`
- **THEN** discovery stops at that directory and does not search for a database in higher ancestors

#### Scenario: No project database exists

- **WHEN** no valid `.dbox/data.db` is found before reaching the filesystem root
- **THEN** the command stops before its main operation and returns the database-not-found result defined by the CLI contract

### Requirement: Automatic migration before data operations

Every catalog command that resolves a project database SHALL apply pending known migrations before its main operation, including `dbox activity schema` and every `dbox activity` data command.

#### Scenario: Run a command against a database with pending migrations

- **WHEN** a command resolves a database that has pending known migrations
- **THEN** the migrations are applied before the requested operation reads or changes data

#### Scenario: Migration fails

- **WHEN** a required migration cannot be applied
- **THEN** the command returns a database error, does not execute the requested operation, and does not delete or replace the database

### Requirement: Project database isolation

Each initialized project SHALL use only its own database and migration history; data created in one project SHALL NOT be visible or mutable from another project.

#### Scenario: Use two independent projects

- **WHEN** an activity is created in project A and the same data command runs in project B
- **THEN** project B does not return or modify the activity from project A
