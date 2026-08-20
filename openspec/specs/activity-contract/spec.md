# activity-contract Specification

## Purpose

Define the fixed activity entity, its public metadata, validation rules, and schema discovery contract exposed by dbox.

## Requirements

### Requirement: Fixed activity fields and rules

The system SHALL expose exactly the fixed `activity` contract with the public fields and rules defined below: `id` is an immutable generated integer; `created_at` is an immutable generated UTC datetime; `type` is required and one of `research`, `implementation`, `bugfix`, or `maintenance`; `title` is required, non-blank, and at most 200 characters; `description` is optional and nullable; and `status` is required and one of `pending`, `in_progress`, or `completed`, defaulting to `completed`.

#### Scenario: Describe the activity contract

- **WHEN** a user requests the activity schema in JSON
- **THEN** the response identifies the `activity` entity and reports each fixed field with its public type, required status where applicable, generated status, mutability, enum values, maximum length, and default value

#### Scenario: Reject invalid enum values

- **WHEN** an input supplies a `type` or `status` value outside its declared set or with different capitalization
- **THEN** validation fails before persistence

#### Scenario: Reject invalid titles

- **WHEN** an input supplies an empty, whitespace-only, or more-than-200-character title
- **THEN** validation fails before persistence

#### Scenario: Apply the default status

- **WHEN** an activity is created without a `status`
- **THEN** the persisted and returned activity has status `completed`

#### Scenario: Generate creation metadata

- **WHEN** an activity is created successfully
- **THEN** the system generates its `id` and its UTC `created_at`, and does not accept either value as caller-controlled data

### Requirement: Schema discovery command

The system SHALL provide `schema` and `--schema` as equivalent commands that resolve the project database, apply pending migrations, and expose the public activity contract without exposing SQLite or EF Core internals.

#### Scenario: Request the human schema

- **WHEN** a user runs `dbox schema`
- **THEN** the system prints a readable description of the `activity` entity and its rules

#### Scenario: Request the JSON schema

- **WHEN** a user runs `dbox schema --json` or `dbox --schema --json`
- **THEN** the system prints the stable JSON contract under `entities.activity.fields` with no database introspection details or auxiliary text

#### Scenario: Use the schema alias

- **WHEN** a user runs `dbox --schema` or `dbox --schema --json`
- **THEN** the system behaves exactly as the corresponding `schema` command

### Requirement: Consistent activity validation

The system SHALL apply the same activity field rules to every create and update input and SHALL validate all input before persistence.

#### Scenario: Preserve generated fields during update

- **WHEN** an update attempts to provide or change `id` or `created_at`
- **THEN** validation fails and the stored generated fields remain unchanged

#### Scenario: Validate before saving

- **WHEN** an add or update input is invalid
- **THEN** the system returns a validation result without executing the persistent write
