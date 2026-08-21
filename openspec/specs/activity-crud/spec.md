# activity-crud Specification

## Purpose

Define the observable create, read, update, and delete operations for the fixed activity entity through the dbox command-line interface.

## Requirements

### Requirement: Create an activity

The system SHALL create an activity from exactly one JSON object supplied through either the required `--json <object>` option or the required `--json-file <path>` option of `dbox activity add`. `--json-file -` SHALL read the JSON object from standard input. The object SHALL contain the writable activity fields and SHALL NOT include generated or unknown fields.

#### Scenario: Create from inline JSON

- **WHEN** a user runs `dbox activity add --json` with a valid object containing every required writable activity field
- **THEN** the system persists and returns the complete activity including generated `id` and `created_at`

#### Scenario: Create from a JSON file

- **WHEN** a user runs `dbox activity add --json-file activity.json` and the file contains a valid create object
- **THEN** the system persists and returns the complete activity including generated `id` and `created_at`

#### Scenario: Create from standard input

- **WHEN** a user pipes a valid create object to `dbox activity add --json-file -`
- **THEN** the system persists and returns the complete activity including generated `id` and `created_at`

#### Scenario: Reject a missing or conflicting create payload source

- **WHEN** a user omits both payload options or supplies both `--json` and `--json-file`
- **THEN** the system returns a validation error and does not persist an activity

#### Scenario: Reject field options and invalid create JSON properties

- **WHEN** a user supplies field options, an unknown property, `id`, or `created_at` to `dbox activity add`
- **THEN** the system returns a validation error and does not persist an activity

### Requirement: List activities

The system SHALL list activities through `dbox activity list`, ordered by `created_at ASC` and then `id ASC`. The command SHALL accept an optional filter object from exactly one of `--json <object>` or `--json-file <path>`; `--json-file -` SHALL read the object from standard input. The filter object SHALL accept only `type`, `status`, `area`, `source`, `effort`, `created_from`, `created_to`, `title`, and `description`.

`type`, `status`, `area`, `source`, and `effort` SHALL match their corresponding field values. `created_from` and `created_to` SHALL be UTC ISO 8601 datetimes with a `Z` offset and SHALL bound `created_at` inclusively; when both are present, `created_from` SHALL not be later than `created_to`. `title` and `description` SHALL match activities whose corresponding non-null field contains the supplied text as a case-insensitive partial search. All supplied filters SHALL combine with logical AND.

The command SHALL apply filtering and ordering before pagination. `--skip` SHALL be a non-negative integer and default to `0`. `--take` SHALL be a non-negative integer and default to `100`. `--all` SHALL remove the result limit and SHALL be mutually exclusive with `--take`. On success, stdout SHALL contain exactly one envelope JSON object with `items`, containing the selected activities, and `pagination`, containing the effective `skip`, `take`, `total`, and `has_more`. `total` SHALL be the number of activities matching the filters before pagination. `take` SHALL be `null` when `--all` is supplied, and `has_more` SHALL be `false` when all matching records are returned.

#### Scenario: List without filters

- **WHEN** a user runs `dbox activity list`
- **THEN** the system returns at most the first 100 activities in `created_at ASC`, `id ASC` order in the response envelope with the matching total and pagination metadata

#### Scenario: List every matching activity

- **WHEN** a user runs `dbox activity list --all`
- **THEN** the system returns every matching activity in order with `pagination.take` set to `null` and `pagination.has_more` set to `false`

#### Scenario: List with combined filters

- **WHEN** a user supplies valid `type`, `status`, `area`, `source`, `effort`, UTC date-range, title, and description filters in the payload
- **THEN** the system returns only activities matching every supplied filter in `created_at ASC`, `id ASC` order

#### Scenario: Paginate an ordered list

- **WHEN** a user runs `dbox activity list --skip 10 --take 10`
- **THEN** the system skips the first ten ordered matching activities, returns at most the next ten in `items`, reports the matching `total`, and sets `has_more` when additional matching activities remain

#### Scenario: List an empty result

- **WHEN** no activity matches the filters
- **THEN** stdout is an envelope with `items` equal to `[]`, `pagination.total` equal to `0`, and `pagination.has_more` equal to `false`

#### Scenario: Reject invalid list input

- **WHEN** a list payload contains an unknown property, an invalid enum value, invalid UTC range value, `created_from` later than `created_to`, or an invalid partial-search value; `--skip` or `--take` is negative or not an integer; both payload options are supplied; or `--all` and `--take` are supplied together
- **THEN** the system returns the JSON validation error defined by the CLI contract and does not return activities

### Requirement: Get one activity

The system SHALL return all public fields for the activity identified by the numeric id supplied to `dbox activity get`.

#### Scenario: Get an existing activity

- **WHEN** a user runs `dbox activity get <id>` for an existing id
- **THEN** the system returns the complete activity

#### Scenario: Get a missing activity

- **WHEN** a user runs `dbox activity get <id>` for an id that does not exist
- **THEN** the system returns the resource-not-found result defined by the CLI contract

### Requirement: Partially update an activity

The system SHALL update only the writable fields supplied in exactly one JSON object from either `--json <object>` or `--json-file <path>` on `dbox activity update <id>`: `type`, `title`, `description`, `status`, `source`, `area`, `result`, `impact`, `effort`, `reference`, and `metadata`. The object SHALL also contain the current positive integer `version` as an expected-version precondition; it SHALL NOT treat that value as a writable field. `--json-file -` SHALL read the object from standard input. The numeric activity ID SHALL remain a positional argument. A successful conditional update SHALL change only the supplied writable fields, set `updated_at` to the current UTC datetime, increment `version` by exactly one, and return the complete updated activity.

#### Scenario: Update selected fields from JSON

- **WHEN** a user runs `dbox activity update 15 --json '{"status":"completed","version":2}'` and activity 15 has version 2
- **THEN** the system changes only the supplied writable field, sets a later `updated_at`, sets version 3, and returns the complete updated activity

#### Scenario: Update selected fields from a JSON file

- **WHEN** a user runs `dbox activity update 15 --json-file update.json` and the file contains a valid update object with the current expected version
- **THEN** the system changes only the supplied fields and returns the complete updated activity

#### Scenario: Reject a missing or invalid expected version

- **WHEN** an update JSON omits `version` or supplies a non-positive integer, non-integer, or non-numeric version
- **THEN** the system returns a JSON validation error and does not persist a change

#### Scenario: Reject a missing or conflicting update payload source

- **WHEN** a user omits both payload options or supplies both `--json` and `--json-file`
- **THEN** the system returns a JSON validation error and does not persist a change

#### Scenario: Clear an optional field

- **WHEN** an update JSON explicitly supplies `reference: null` or `metadata: null` and the current expected version
- **THEN** the system stores null for the supplied optional field, preserves all omitted fields, and increments the version

#### Scenario: Reject an empty or invalid update

- **WHEN** an update JSON contains no writable field, an unknown property, `id`, `created_at`, `updated_at`, or an invalid field value including null, empty, or whitespace for a required field
- **THEN** the system returns a validation error and does not persist a change

#### Scenario: Reject a stale update

- **WHEN** an update supplies a version that no longer equals the persisted activity version
- **THEN** the system returns the conflict error defined by the CLI contract and does not overwrite the persisted activity

#### Scenario: Update a missing activity

- **WHEN** an update targets an id that does not exist
- **THEN** the system returns the resource-not-found result defined by the CLI contract

### Requirement: Delete an activity

The system SHALL delete an existing activity only through `dbox activity delete
<id> --yes`; it SHALL NOT prompt interactively or move data to a recycle bin.
The `--yes` option SHALL be required for a persistent deletion. The command
SHALL accept `--dry-run` as a non-mutating alternative that does not require
`--yes`, resolves and validates the existing activity without applying
migrations, and returns a preview object with the complete `activity`, its `id`,
`deleted: false`, and `dry_run: true`. When both options are supplied,
`--dry-run` SHALL take precedence and no deletion SHALL occur.

#### Scenario: Delete an existing activity after explicit confirmation

- **WHEN** a user runs `dbox activity delete <id> --yes` for an existing activity
- **THEN** the system removes the activity and returns its id with `deleted: true` in JSON output

#### Scenario: Reject an unconfirmed deletion

- **WHEN** a user runs `dbox activity delete <id>` without `--yes` or `--dry-run`
- **THEN** the system returns the JSON validation error defined by the CLI contract and does not persist a change

#### Scenario: Preview an existing deletion

- **WHEN** a user runs `dbox activity delete <id> --dry-run` for an existing activity
- **THEN** the system returns the complete activity with its id, `deleted: false`, and `dry_run: true` without deleting, migrating, or otherwise persisting a change

#### Scenario: Preview a missing deletion

- **WHEN** a user runs `dbox activity delete <id> --dry-run` for an id that does not exist
- **THEN** the system returns the same resource-not-found result as `get` without persisting a change

#### Scenario: Prefer a dry run when confirmation is also supplied

- **WHEN** a user runs `dbox activity delete <id> --yes --dry-run`
- **THEN** the system performs the non-mutating preview and does not delete the activity

#### Scenario: Delete a missing activity

- **WHEN** a user runs `dbox activity delete <id> --yes` for an id that does not exist
- **THEN** the system returns the same resource-not-found result as `get`
