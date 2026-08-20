## ADDED Requirements

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

## MODIFIED Requirements

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
