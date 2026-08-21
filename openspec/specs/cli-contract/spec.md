# cli-contract Specification

## Purpose

Define stable command-line output, error, alias, and exit-code behavior so people, scripts, and agents can use dbox without parsing implementation details.

## Requirements

### Requirement: Root infrastructure and catalog hierarchy

The root `dbox` command SHALL expose only shared infrastructure operations and
available catalog groups. `init`, `context`, `backup`, and `doctor` SHALL be root
infrastructure operations, and `activity` SHALL be the available catalog group.
Catalog-specific operations SHALL NOT be exposed directly at the root.

#### Scenario: Discover root operations

- **WHEN** a user requests root help
- **THEN** the help presents `init`, `context`, `backup`, `doctor`, and the `activity` catalog group, and does not present activity CRUD or schema commands as root commands

#### Scenario: Reject a removed flat activity command

- **WHEN** a user invokes `dbox schema`, `dbox add`, `dbox list`, `dbox get`, `dbox update`, or `dbox delete`
- **THEN** the system returns a command syntax validation error and does not perform an activity operation

### Requirement: Catalog-specific schema aliases

The activity schema SHALL be available only through `dbox activity schema`.
Neither `dbox activity --schema` nor a root-level `dbox --schema` alias SHALL
be accepted.

#### Scenario: Request the canonical activity schema command

- **WHEN** a user invokes `dbox activity schema`
- **THEN** the system resolves the project database and returns the activity schema response

#### Scenario: Reject a removed schema alias

- **WHEN** a user invokes `dbox activity --schema` or `dbox --schema`
- **THEN** the system returns a JSON command syntax validation error and does not query a project database

### Requirement: Maintenance command JSON responses

Successful root maintenance commands SHALL write exactly one JSON object to
stdout. `dbox backup` SHALL identify the created backup file, and `dbox doctor`
SHALL report database existence, opening status, SQLite integrity status,
pending known migrations, and inspected permissions. Neither command SHALL
write diagnostics or auxiliary text outside its JSON response.

#### Scenario: Return a backup result

- **WHEN** `dbox backup` successfully creates a backup
- **THEN** stdout contains exactly one JSON object that identifies the created backup file

#### Scenario: Return a doctor diagnostic result

- **WHEN** `dbox doctor` completes its read-only diagnostic checks
- **THEN** stdout contains exactly one JSON object with the documented maintenance diagnostic fields

### Requirement: Selectable successful output

Every operational command SHALL write exactly one valid JSON value to stdout on
success, without headers, diagnostics, or auxiliary text. The CLI SHALL NOT
accept `--output`, and JSON output SHALL NOT require a format-selection option.

#### Scenario: Return an activity as JSON by default

- **WHEN** a user runs a successful activity command without an output-format option
- **THEN** stdout contains exactly one JSON value representing its documented response

#### Scenario: Reject a removed output option

- **WHEN** a user supplies `--output`, `--output text`, or `--output json`
- **THEN** the system returns a JSON command syntax validation error and does not perform the requested operation

### Requirement: Stable initialization responses

The `init` command SHALL report one of `initialized`, `already_initialized`, or
`migrated` as a JSON object and SHALL identify the database as `.dbox/data.db`.

#### Scenario: Return initialized status

- **WHEN** a new database is initialized with `dbox init`
- **THEN** stdout contains an object with `database: ".dbox/data.db"` and `status: "initialized"`

#### Scenario: Return existing initialization statuses

- **WHEN** `dbox init` finds an up-to-date database or applies pending migrations
- **THEN** stdout contains an object with status `already_initialized` or `migrated` respectively

### Requirement: Structured errors and exit codes

Expected operational and command syntax errors SHALL be written to stderr as
exactly one valid JSON object with an `error` property containing a stable
`code`, `message`, and optional `details` array. Stdout SHALL be empty for
errors. The system SHALL use exit code `0` for success, `1` for unexpected
errors, `2` for validation or syntax errors, `3` for missing resources, and
`4` for missing databases, migration failures, or other database errors.

#### Scenario: Return a validation error

- **WHEN** input values, a JSON payload, or command syntax are invalid
- **THEN** `stderr` contains only a JSON error with code `validation_error`, `stdout` is empty, and the exit code is `2`

#### Scenario: Return a missing resource error

- **WHEN** a requested activity does not exist
- **THEN** stderr contains only a JSON error with code `resource_not_found`, stdout is empty, and the exit code is `3`

#### Scenario: Return a missing database error

- **WHEN** a data command cannot find a valid project database
- **THEN** stderr contains only a JSON error with code `database_not_found`, stdout is empty, and the exit code is `4`

#### Scenario: Return a database failure

- **WHEN** opening, migrating, or persisting the project database fails
- **THEN** stderr contains only a JSON error with code `database_error`, stdout is empty, and the exit code is `4`

### Requirement: Exclusive JSON payload sources

Every activity command that accepts a JSON payload SHALL accept `--json <object>` and `--json-file <path>` as mutually exclusive alternatives. `--json-file -` SHALL read one JSON object from standard input. A file payload SHALL be read as UTF-8. The mutually exclusive option error SHALL use the message `Specify either '--json' or '--json-file', not both.`. An unreadable file SHALL use the message `Unable to read JSON input.`. Invalid JSON or a JSON value other than an object SHALL use the message `JSON input must be a valid JSON object.`. Each of these failures SHALL return the JSON validation-error shape and exit code used for other invalid input.

#### Scenario: Reject conflicting JSON payload options

- **WHEN** a user supplies `--json` and `--json-file` to an activity command that accepts JSON
- **THEN** stderr contains only a `validation_error` JSON object with message `Specify either '--json' or '--json-file', not both.`, stdout is empty, and the exit code is `2`

#### Scenario: Reject unreadable JSON input

- **WHEN** a user supplies a nonexistent or unreadable path to `--json-file`
- **THEN** stderr contains only a `validation_error` JSON object with message `Unable to read JSON input.`, stdout is empty, and the exit code is `2`

#### Scenario: Reject invalid JSON input

- **WHEN** the selected inline, file, or standard-input payload is malformed or is not a JSON object
- **THEN** stderr contains only a `validation_error` JSON object with message `JSON input must be a valid JSON object.`, stdout is empty, and the exit code is `2`

### Requirement: Deterministic query-option validation

The CLI SHALL reject incompatible or invalid query options as `validation_error` results before executing the activity query. Specifically, `dbox activity list --all --take <integer>` SHALL be rejected with the message `Options '--all' and '--take' cannot be used together.`. Invalid pagination or filter values SHALL preserve the structured-error output and exit-code behavior defined by this contract.

#### Scenario: Reject an unbounded list with an explicit take

- **WHEN** a user runs `dbox activity list --all --take 100`
- **THEN** stderr contains only a `validation_error` JSON object with message `Options '--all' and '--take' cannot be used together.`, stdout is empty, and the exit code is `2`

### Requirement: Native command help

The CLI SHALL preserve its native human-readable `--help` output for the root,
catalog, and leaf commands. Help output SHALL list available commands,
arguments, and options and SHALL NOT perform a database operation.

#### Scenario: Discover list pagination options

- **WHEN** a user runs `dbox activity list --help`
- **THEN** the help output describes `--json`, `--json-file`, `--skip`, `--take`, and `--all`

#### Scenario: Discover JSON file input options

- **WHEN** a user runs `dbox activity add --help`, `dbox activity count --help`, or `dbox activity update --help`
- **THEN** the help output describes `--json-file` as an alternative to `--json`
