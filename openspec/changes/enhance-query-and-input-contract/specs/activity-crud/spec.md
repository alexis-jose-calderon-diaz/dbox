## MODIFIED Requirements

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
- **THEN** the system returns the JSON validation error defined by the CLI contract and does not persist an activity

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

### Requirement: Partially update an activity
The system SHALL update only the writable fields supplied in exactly one JSON object from either `--json <object>` or `--json-file <path>` on `dbox activity update <id>`: `type`, `title`, `description`, `status`, `source`, `area`, `result`, `impact`, `effort`, `reference`, and `metadata`. `--json-file -` SHALL read the object from standard input. The numeric activity ID SHALL remain a positional argument.

#### Scenario: Update selected fields from inline JSON
- **WHEN** a user runs `dbox activity update 15 --json '{"status":"completed"}'`
- **THEN** the system changes only the supplied field and returns the complete updated activity

#### Scenario: Update selected fields from a JSON file
- **WHEN** a user runs `dbox activity update 15 --json-file update.json` and the file contains a valid update object
- **THEN** the system changes only the supplied fields and returns the complete updated activity

#### Scenario: Reject a missing or conflicting update payload source
- **WHEN** a user omits both payload options or supplies both `--json` and `--json-file`
- **THEN** the system returns a JSON validation error and does not persist a change

#### Scenario: Clear an optional field
- **WHEN** an update JSON explicitly supplies `reference: null` or `metadata: null`
- **THEN** the system stores null for the supplied optional field

#### Scenario: Reject an empty or invalid update
- **WHEN** an update JSON contains no writable field, an unknown property, `id`, `created_at`, or an invalid field value including null, empty, or whitespace for a required field
- **THEN** the system returns a validation error and does not persist a change

#### Scenario: Update a missing activity
- **WHEN** an update targets an id that does not exist
- **THEN** the system returns the resource-not-found result defined by the CLI contract
