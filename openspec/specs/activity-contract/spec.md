# activity-contract Specification

## Purpose

Define the fixed activity entity, its public metadata, validation rules, and schema discovery contract exposed by dbox.

## Requirements

### Requirement: Fixed activity fields and rules

The system SHALL expose exactly the fixed `activity` contract with immutable generated integer `id`; immutable generated UTC datetimes `created_at` and `updated_at`; immutable generated positive integer `version`; required, non-blank strings `type`, `title`, `description`, `status`, `source`, `area`, `result`, `impact`, and `effort`; and optional nullable `reference` and `metadata`. `updated_at` SHALL equal `created_at` on creation and change only after a successful update. `version` SHALL start at `1` and increment once after each successful update. `type`, `source`, and `area` SHALL be extensible strings rather than closed enums. `status` SHALL remain one of `pending`, `in_progress`, or `completed`. `effort` SHALL be one of `low`, `medium`, `high`, or `very-high`. `title` SHALL be non-blank and at most 200 characters. `reference` SHALL be a nullable string. `metadata` SHALL be a nullable JSON object and SHALL contain supplemental external-tool information rather than replace the required activity fields. The system SHALL not expose dedicated fields for OpenSpec, commit, branch, issue, or pull request references.

#### Scenario: Describe the activity contract

- **WHEN** a user requests the activity schema in JSON
- **THEN** the response identifies the `activity` entity and reports each fixed field with its public type, required status where applicable, generated status, mutability, enum values, maximum length, default value, and description

#### Scenario: Reject invalid controlled values

- **WHEN** an input supplies a `status` or `effort` value outside its declared set or with different capitalization
- **THEN** validation fails before persistence

#### Scenario: Reject invalid required values

- **WHEN** an input omits a required writable field or supplies null, an empty string, or whitespace for one
- **THEN** validation fails before persistence

#### Scenario: Reject an oversized title

- **WHEN** an input supplies a title longer than 200 characters
- **THEN** validation fails before persistence

#### Scenario: Validate optional JSON metadata

- **WHEN** an input supplies `metadata`
- **THEN** the system accepts it only when it is a valid JSON object

#### Scenario: Generate creation metadata

- **WHEN** an activity is created successfully
- **THEN** the system generates its `id`, UTC `created_at`, UTC `updated_at`, and `version` equal to `1`, and does not accept any of them as caller-controlled create data

#### Scenario: Report modification metadata

- **WHEN** a user requests an existing activity through `get` or receives it from a successful create or update
- **THEN** the complete activity includes its immutable `updated_at` and `version` values

### Requirement: Schema discovery command

The system SHALL provide only `dbox activity schema` to expose the public
activity contract of the installed CLI. The command SHALL return the stable JSON
contract under `entities.activity.fields` without SQLite or EF Core internals or
auxiliary text, and SHALL NOT resolve, open, migrate, create, or otherwise
require `.dbox/data.db`.

#### Scenario: Request the activity schema without a project database

- **WHEN** a user runs `dbox activity schema` from a directory with no `.dbox/data.db`
- **THEN** stdout contains the stable JSON activity contract under `entities.activity.fields` and the command exits successfully

#### Scenario: Request the activity schema without migrating a project database

- **WHEN** a user runs `dbox activity schema` from a directory that contains a project database with pending migrations
- **THEN** the command returns the installed CLI contract without applying migrations or modifying the database

#### Scenario: Reject schema format and alias options

- **WHEN** a user runs `dbox activity schema --json` or `dbox activity --schema`
- **THEN** the system returns the JSON command syntax validation error defined by the CLI contract

### Requirement: Consistent activity validation

The system SHALL apply the same activity field rules to every create, update, and import input and SHALL validate all input before persistence. `id`, `created_at`, `updated_at`, and `version` SHALL be generated and immutable in ordinary create and update data; an import record SHALL instead provide every public field exactly as exported and validate its generated fields as positive integer or UTC datetime values, as applicable. An update MAY omit writable fields to preserve their stored values, but it SHALL not set a required field to null, an empty string, or whitespace. An update SHALL allow `reference` or `metadata` to be explicitly set to null.

#### Scenario: Preserve generated fields during update

- **WHEN** an update attempts to provide or change `id`, `created_at`, or `updated_at`
- **THEN** validation fails and the stored generated fields remain unchanged

#### Scenario: Clear an optional field during update

- **WHEN** an update explicitly supplies `reference: null` or `metadata: null`
- **THEN** the system stores a null value for that optional field

#### Scenario: Validate before saving

- **WHEN** an add, update, or import input is invalid
- **THEN** the system returns a validation result without executing the persistent write

#### Scenario: Validate imported generated metadata

- **WHEN** an import record provides a non-positive `id` or `version`, or a `created_at` or `updated_at` value that is not a UTC datetime
- **THEN** validation fails before the import transaction begins
