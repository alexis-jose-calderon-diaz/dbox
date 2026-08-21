## MODIFIED Requirements

### Requirement: Partially update an activity
The system SHALL update only the writable fields supplied in the required `--json` object of `dbox activity update <id>`: `type`, `title`, `description`, `status`, `source`, `area`, `result`, `impact`, `effort`, `reference`, and `metadata`. The object SHALL also contain the current positive integer `version` as an expected-version precondition; it SHALL NOT treat that value as a writable field. The numeric activity ID SHALL remain a positional argument. A successful conditional update SHALL change only the supplied writable fields, set `updated_at` to the current UTC datetime, increment `version` by exactly one, and return the complete updated activity.

#### Scenario: Update selected fields from JSON
- **WHEN** a user runs `dbox activity update 15 --json '{"status":"completed","version":2}'` and activity 15 has version 2
- **THEN** the system changes only the supplied writable field, sets a later `updated_at`, sets version 3, and returns the complete updated activity

#### Scenario: Reject a missing update payload
- **WHEN** a user runs `dbox activity update 15` without `--json`
- **THEN** the system returns a JSON validation error and does not persist a change

#### Scenario: Reject a missing or invalid expected version
- **WHEN** an update JSON omits `version` or supplies a non-positive integer, non-integer, or non-numeric version
- **THEN** the system returns a JSON validation error and does not persist a change

#### Scenario: Clear an optional field
- **WHEN** an update JSON explicitly supplies `reference: null` or `metadata: null` and the current expected version
- **THEN** the system stores null for the supplied optional field and increments the version

#### Scenario: Reject an empty or invalid update
- **WHEN** an update JSON contains no writable field, an unknown property, `id`, `created_at`, `updated_at`, or an invalid field value including null, empty, or whitespace for a required field
- **THEN** the system returns a validation error and does not persist a change

#### Scenario: Reject a stale update
- **WHEN** an update supplies a version that no longer equals the persisted activity version
- **THEN** the system returns the conflict error defined by the CLI contract and does not overwrite the persisted activity

#### Scenario: Update a missing activity
- **WHEN** an update targets an id that does not exist
- **THEN** the system returns the resource-not-found result defined by the CLI contract
