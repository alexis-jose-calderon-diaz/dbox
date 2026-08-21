## MODIFIED Requirements

### Requirement: Create an activity
The system SHALL create an activity only from a JSON object supplied through the required `--json` option of `dbox activity add`. The object SHALL contain every required writable activity field: `type`, `title`, `description`, `status`, `source`, `area`, `result`, `impact`, and `effort`. It MAY include `reference` and `metadata`, and SHALL NOT include generated or unknown fields.

#### Scenario: Create from complete JSON
- **WHEN** a user runs `dbox activity add --json` with valid values for every required writable field and optional metadata
- **THEN** the system persists and returns the complete activity, including generated `id` and `created_at` and the supplied optional fields

#### Scenario: Reject a missing create payload
- **WHEN** a user runs `dbox activity add` without `--json`
- **THEN** the system returns a validation error and does not persist an activity

#### Scenario: Reject incomplete or invalid create JSON
- **WHEN** a create payload omits a required writable field, supplies an invalid field value, contains an unknown property, `id`, or `created_at`
- **THEN** the system returns a validation error and does not persist an activity

### Requirement: List activities
The system SHALL list activities through `dbox activity list`, ordered by `created_at ASC` and then `id ASC`. The command SHALL accept optional `type` and `status` filters only through a `--json <object>` payload, and optional non-negative integer `--skip` and `--take` options. The system SHALL apply the order before applying pagination and SHALL return the complete expanded activity contract for each result.

#### Scenario: List without filters
- **WHEN** a user runs `dbox activity list`
- **THEN** the system returns every complete activity in `created_at ASC`, `id ASC` order as a JSON array

#### Scenario: List with combined filters
- **WHEN** a user supplies a non-blank `type` and a valid `status` in the `--json` payload
- **THEN** the system returns only activities matching both filters in `created_at ASC`, `id ASC` order

#### Scenario: Paginate an ordered list
- **WHEN** a user runs `dbox activity list --skip 10 --take 10`
- **THEN** the system skips the first ten ordered activities and returns at most the next ten activities

#### Scenario: List an empty result
- **WHEN** no activity matches the filters
- **THEN** stdout is the JSON array `[]`

#### Scenario: Reject invalid list input
- **WHEN** a list payload contains an unknown property, a non-string or blank `type`, an invalid `status`, or `--skip` or `--take` is negative or not an integer
- **THEN** the system returns the JSON validation error defined by the CLI contract and does not return activities

### Requirement: Partially update an activity
The system SHALL update only the writable fields supplied in the required `--json` object of `dbox activity update <id>`: `type`, `title`, `description`, `status`, `source`, `area`, `result`, `impact`, `effort`, `reference`, and `metadata`. The numeric activity ID SHALL remain a positional argument.

#### Scenario: Update selected fields from JSON
- **WHEN** a user runs `dbox activity update 15 --json` with valid values for one or more writable fields
- **THEN** the system changes only the supplied fields and returns the complete updated activity

#### Scenario: Reject a missing update payload
- **WHEN** a user runs `dbox activity update 15` without `--json`
- **THEN** the system returns a JSON validation error and does not persist a change

#### Scenario: Clear optional fields
- **WHEN** an update JSON explicitly supplies `reference: null` or `metadata: null`
- **THEN** the system stores null for the supplied optional field and preserves all omitted fields

#### Scenario: Reject an empty or invalid update
- **WHEN** an update JSON contains no writable field, an unknown property, `id`, `created_at`, an invalid field value, or null, empty, or whitespace for a required field
- **THEN** the system returns a validation error and does not persist a change

#### Scenario: Update a missing activity
- **WHEN** an update targets an id that does not exist
- **THEN** the system returns the resource-not-found result defined by the CLI contract
