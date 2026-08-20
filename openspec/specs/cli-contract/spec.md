# cli-contract Specification

## Purpose

Define stable command-line output, error, alias, and exit-code behavior so people, scripts, and agents can use dbox without parsing implementation details.

## Requirements

### Requirement: Root infrastructure and catalog hierarchy

The root `dbox` command SHALL expose only shared infrastructure operations and available catalog groups. `init` SHALL remain a root operation, and `activity` SHALL be the available catalog group. Catalog-specific operations SHALL NOT be exposed directly at the root.

#### Scenario: Discover root operations

- **WHEN** a user requests root help
- **THEN** the help presents `init` and the `activity` catalog group, and does not present activity CRUD or schema commands as root commands

#### Scenario: Reject a removed flat activity command

- **WHEN** a user invokes `dbox schema`, `dbox add`, `dbox list`, `dbox get`, `dbox update`, or `dbox delete`
- **THEN** the system returns a command syntax validation error and does not perform an activity operation

### Requirement: Catalog-specific schema aliases

The `--schema` alias SHALL be scoped to its catalog group and SHALL NOT be accepted as a root-level alias for the activity schema.

#### Scenario: Reject the removed root schema alias

- **WHEN** a user invokes `dbox --schema` or `dbox --schema --json`
- **THEN** the system returns a command syntax validation error and does not query a project database

### Requirement: Selectable successful output

All commands that produce data SHALL accept `--output text` and `--output json`, default to `text`, and write successful output only to `stdout`.

#### Scenario: Use default text output

- **WHEN** a data command runs without `--output`
- **THEN** the system emits its documented human-readable response on `stdout`

#### Scenario: Use JSON output

- **WHEN** a data command runs with `--output json`
- **THEN** the system emits exactly one valid JSON value on `stdout` without headers, diagnostics, or auxiliary text

#### Scenario: Render list text output

- **WHEN** `dbox activity list` uses text output
- **THEN** the activity rows include the columns `ID`, `CREATED_AT`, `TYPE`, `STATUS`, and `TITLE`

### Requirement: Stable initialization responses

The `init` command SHALL report one of `initialized`, `already_initialized`, or `migrated` and SHALL identify the database as `.dbox/data.db` in both output formats.

#### Scenario: Return initialized status as JSON

- **WHEN** a new database is initialized with `dbox init --output json`
- **THEN** `stdout` contains an object with `database: ".dbox/data.db"` and `status: "initialized"`

#### Scenario: Return human initialization messages

- **WHEN** `dbox init` initializes, finds an up-to-date database, or migrates an existing database
- **THEN** `stdout` contains respectively `Database initialized: .dbox/data.db`, `Database already initialized: .dbox/data.db`, or `Database migrated: .dbox/data.db`

### Requirement: Structured errors and exit codes

Expected errors SHALL be written to `stderr`; JSON error output SHALL contain only a valid object with an `error` property containing a stable `code`, `message`, and optional `details` array. The system SHALL use exit code `0` for success, `1` for unexpected errors, `2` for validation or syntax errors, `3` for missing resources, and `4` for missing databases, migration failures, or other database errors.

#### Scenario: Return a validation error

- **WHEN** input values or command syntax are invalid and `--output json` is selected
- **THEN** `stderr` contains only a JSON error with code `validation_error`, `stdout` is empty, and the exit code is `2`

#### Scenario: Return a missing resource error

- **WHEN** a requested activity does not exist
- **THEN** `stderr` identifies the missing resource, `stdout` is empty, and the exit code is `3`

#### Scenario: Return a missing database error

- **WHEN** a data command cannot find a valid project database
- **THEN** `stderr` contains `No dbox database found.`, followed by `Run 'dbox init' to initialize this directory.`, and the exit code is `4`

#### Scenario: Return a database failure

- **WHEN** opening, migrating, or persisting the project database fails
- **THEN** the system writes a database error to `stderr`, does not write a successful response to `stdout`, and exits with code `4`

### Requirement: Stable human resource messages

The text interface SHALL use the documented messages for missing and deleted activities.

#### Scenario: Report a missing activity in text

- **WHEN** `get` or `delete` targets a missing activity with text output
- **THEN** `stderr` contains `Activity <id> not found.`

#### Scenario: Report a deleted activity in text

- **WHEN** `delete` successfully removes an activity with text output
- **THEN** `stdout` contains `Activity <id> deleted.`
