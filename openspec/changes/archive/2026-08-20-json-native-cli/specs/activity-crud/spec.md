## MODIFIED Requirements

### Requirement: Create an activity
The system SHALL create an activity only from a JSON object supplied through
the required `--json` option of `dbox activity add`. The object SHALL contain
the writable activity fields and SHALL NOT include generated or unknown fields.

#### Scenario: Create from JSON
- **WHEN** a user runs `dbox activity add --json '{"type":"research","title":"Investigate"}'`
- **THEN** the system persists and returns the complete activity including generated `id` and `created_at`

#### Scenario: Reject a missing create payload
- **WHEN** a user runs `dbox activity add` without `--json`
- **THEN** the system returns a JSON validation error and does not persist an activity

#### Scenario: Reject field options and invalid create JSON properties
- **WHEN** a user supplies field options, an unknown property, `id`, or `created_at` to `dbox activity add`
- **THEN** the system returns a JSON validation error and does not persist an activity

### Requirement: List activities
The system SHALL list activities through `dbox activity list`, ordered by
`created_at ASC` and then `id ASC`. The command SHALL accept optional `type`
and `status` filters only through a `--json <object>` payload, and optional
non-negative integer `--skip` and `--take` options. The system SHALL apply the
order before applying pagination.

#### Scenario: List without filters
- **WHEN** a user runs `dbox activity list`
- **THEN** the system returns every activity in `created_at ASC`, `id ASC` order as a JSON array

#### Scenario: List with combined JSON filters
- **WHEN** a user supplies valid `type` and `status` filters in the `--json` payload
- **THEN** the system returns only activities matching both filters in `created_at ASC`, `id ASC` order

#### Scenario: Paginate an ordered list
- **WHEN** a user runs `dbox activity list --skip 10 --take 10`
- **THEN** the system skips the first ten ordered activities and returns at most the next ten activities

#### Scenario: List an empty result
- **WHEN** no activity matches the filters or pagination window
- **THEN** stdout is the JSON array `[]`

#### Scenario: Reject invalid list input
- **WHEN** a list payload contains an unknown property or invalid filter value, or `--skip` or `--take` is negative or not an integer
- **THEN** the system returns the JSON validation error defined by the CLI contract and does not return activities

### Requirement: Partially update an activity
The system SHALL update only the writable fields supplied in the required
`--json` object of `dbox activity update <id>`: `type`, `title`,
`description`, and `status`. The numeric activity ID SHALL remain a positional
argument.

#### Scenario: Update selected fields from JSON
- **WHEN** a user runs `dbox activity update 15 --json '{"status":"completed"}'`
- **THEN** the system changes only the supplied field and returns the complete updated activity

#### Scenario: Reject a missing update payload
- **WHEN** a user runs `dbox activity update 15` without `--json`
- **THEN** the system returns a JSON validation error and does not persist a change

#### Scenario: Clear a description
- **WHEN** an update JSON explicitly supplies `description: null`
- **THEN** the system stores a null description

#### Scenario: Reject an empty or invalid update
- **WHEN** an update JSON contains no writable field, an unknown property, `id`, `created_at`, or an invalid field value
- **THEN** the system returns a JSON validation error and does not persist a change

#### Scenario: Update a missing activity
- **WHEN** an update targets an ID that does not exist
- **THEN** the system returns the resource-not-found result defined by the CLI contract
