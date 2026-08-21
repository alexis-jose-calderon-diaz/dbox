## ADDED Requirements

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

## MODIFIED Requirements

### Requirement: Native command help
The CLI SHALL preserve its native human-readable `--help` output for the root, catalog, and leaf commands. Help output SHALL list available commands, arguments, and options and SHALL NOT perform a database operation.

#### Scenario: Discover list query options
- **WHEN** a user runs `dbox activity list --help`
- **THEN** the help output describes `--json`, `--json-file`, `--skip`, `--take`, and `--all`

#### Scenario: Discover JSON file input options
- **WHEN** a user runs `dbox activity add --help`, `dbox activity count --help`, or `dbox activity update --help`
- **THEN** the help output describes `--json-file` as an alternative to `--json`
