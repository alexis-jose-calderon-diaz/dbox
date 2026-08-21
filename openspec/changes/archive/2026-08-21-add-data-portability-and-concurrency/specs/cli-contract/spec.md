## MODIFIED Requirements

### Requirement: Selectable successful output
Every operational command other than `dbox activity export --format jsonl` SHALL write exactly one valid JSON value to stdout on success, without headers, diagnostics, or auxiliary text. `dbox activity export` SHALL produce its portable data on stdout: its default `json` format is exactly one JSON array, while its `jsonl` format is one valid JSON object per line and no auxiliary output. The CLI SHALL NOT accept `--output`, and JSON output for commands other than the documented export format SHALL NOT require a format-selection option.

#### Scenario: Return an activity as JSON by default
- **WHEN** a user runs a successful activity command without an output-format option
- **THEN** stdout contains exactly one JSON value representing its documented response

#### Scenario: Return JSONL only for portable export
- **WHEN** a user runs `dbox activity export --format jsonl`
- **THEN** stdout contains only the documented line-delimited portable activity records

#### Scenario: Reject a removed output option
- **WHEN** a user supplies `--output`, `--output text`, or `--output json`
- **THEN** the system returns a JSON command syntax validation error and does not perform the requested operation

### Requirement: Structured errors and exit codes
Expected operational and command syntax errors SHALL be written to stderr as exactly one valid JSON object with an `error` property containing a stable `code`, `message`, and optional `details` array. Stdout SHALL be empty for errors. The system SHALL use exit code `0` for success, `1` for unexpected errors, `2` for validation or syntax errors, `3` for missing resources or request conflicts, and `4` for missing databases, migration failures, I/O failures, or other database errors. A stale expected version or an import ID collision SHALL return `conflict_error` with exit code `3`; failure to open or read an import file SHALL return `io_error` with exit code `4`.

#### Scenario: Return a validation error
- **WHEN** input values, a JSON payload, or command syntax are invalid
- **THEN** `stderr` contains only a JSON error with code `validation_error`, `stdout` is empty, and the exit code is `2`

#### Scenario: Return a missing resource error
- **WHEN** a requested activity does not exist
- **THEN** stderr contains only a JSON error with code `resource_not_found`, stdout is empty, and the exit code is `3`

#### Scenario: Return a conflict error
- **WHEN** a conditional update is stale or an import conflicts with an existing or repeated activity ID
- **THEN** stderr contains only a JSON error with code `conflict_error`, stdout is empty, and the exit code is `3`

#### Scenario: Return a missing database error
- **WHEN** a data command cannot find a valid project database
- **THEN** stderr contains only a JSON error with code `database_not_found`, stdout is empty, and the exit code is `4`

#### Scenario: Return an I/O error
- **WHEN** an import file cannot be opened or read
- **THEN** stderr contains only a JSON error with code `io_error`, stdout is empty, and the exit code is `4`

#### Scenario: Return a database failure
- **WHEN** opening, migrating, or persisting the project database fails
- **THEN** stderr contains only a JSON error with code `database_error`, stdout is empty, and the exit code is `4`
