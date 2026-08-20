## MODIFIED Requirements

### Requirement: Create an activity

The system SHALL create an activity from either `dbox activity add` command options or one JSON object, but SHALL reject a request that mixes both input forms.

#### Scenario: Create from options

- **WHEN** a user runs `dbox activity add` with valid `--type` and `--title` options with optional description and status
- **THEN** the system persists and returns the complete activity including generated `id` and `created_at`

#### Scenario: Create from JSON

- **WHEN** a user runs `dbox activity add` with a valid `--json` object containing the writable activity fields
- **THEN** the system persists and returns the complete activity

#### Scenario: Reject mixed create input

- **WHEN** a user combines `--json` with one or more activity field options in `dbox activity add`
- **THEN** the system returns a validation error and does not persist an activity

#### Scenario: Reject unknown or read-only JSON properties

- **WHEN** the create JSON contains an unknown property, `id`, or `created_at`
- **THEN** the system returns a validation error and does not persist an activity

### Requirement: List activities

The system SHALL list activities through `dbox activity list` in descending `id` order and SHALL support optional `--type` and `--status` filters that can be combined.

#### Scenario: List without filters

- **WHEN** a user runs `dbox activity list`
- **THEN** the system returns every activity ordered by `id DESC`

#### Scenario: List with combined filters

- **WHEN** a user supplies valid `--type` and `--status` filters to `dbox activity list`
- **THEN** the system returns only activities matching both filters in descending `id` order

#### Scenario: List an empty result

- **WHEN** no activity matches the filters
- **THEN** the JSON result is an empty array and the text result contains no activity rows

### Requirement: Get one activity

The system SHALL return all public fields for the activity identified by the numeric id supplied to `dbox activity get`.

#### Scenario: Get an existing activity

- **WHEN** a user runs `dbox activity get <id>` for an existing id
- **THEN** the system returns the complete activity

#### Scenario: Get a missing activity

- **WHEN** a user runs `dbox activity get <id>` for an id that does not exist
- **THEN** the system returns the resource-not-found result defined by the CLI contract

### Requirement: Partially update an activity

The system SHALL update only the writable fields supplied through `dbox activity update`: `type`, `title`, `description`, and `status`.

#### Scenario: Update selected fields from options

- **WHEN** a user supplies one or more writable update options to `dbox activity update`
- **THEN** the system changes only those fields and returns the complete updated activity

#### Scenario: Update selected fields from JSON

- **WHEN** a user supplies a JSON object containing one or more writable fields to `dbox activity update`
- **THEN** the system changes only those fields and preserves omitted writable fields, `id`, and `created_at`

#### Scenario: Clear a description

- **WHEN** an update JSON explicitly supplies `description: null`
- **THEN** the system stores a null description

#### Scenario: Reject an empty update

- **WHEN** an update contains no writable field
- **THEN** the system returns a validation error and does not persist a change

#### Scenario: Update a missing activity

- **WHEN** an update targets an id that does not exist
- **THEN** the system returns the resource-not-found result defined by the CLI contract

### Requirement: Delete an activity

The system SHALL delete an existing activity through `dbox activity delete` without interactive confirmation.

#### Scenario: Delete an existing activity

- **WHEN** a user runs `dbox activity delete <id>` for an existing activity
- **THEN** the system removes the activity and returns its id with `deleted: true` in JSON output

#### Scenario: Delete a missing activity

- **WHEN** a user runs `dbox activity delete <id>` for an id that does not exist
- **THEN** the system returns the same resource-not-found result as `get`
