# activity-contract Specification

## Purpose

Define the fixed activity entity, its public metadata, validation rules, and schema discovery contract exposed by dbox.

## Requirements

### Requirement: Fixed activity fields and rules

The system SHALL expose exactly the fixed `activity` contract with these public fields: immutable generated integer `id`; immutable generated UTC datetime `created_at`; required, non-blank strings `type`, `title`, `description`, `status`, `source`, `area`, `result`, `impact`, and `effort`; and optional nullable `reference` and `metadata`. `type`, `source`, and `area` SHALL be extensible strings rather than closed enums. `status` SHALL remain one of `pending`, `in_progress`, or `completed`. `effort` SHALL be one of `low`, `medium`, `high`, or `very-high`. `title` SHALL be non-blank and at most 200 characters. `reference` SHALL be a nullable string. `metadata` SHALL be a nullable JSON object and SHALL contain supplemental external-tool information rather than replace the required activity fields. The system SHALL not expose dedicated fields for OpenSpec, commit, branch, issue, or pull request references.

#### Scenario: Describe the complete activity contract

- **WHEN** a user requests the activity schema in JSON
- **THEN** the response identifies all fixed fields and reports each field's name, public type, required status, generated status, mutability, applicable allowed values, and a brief description

#### Scenario: Reject invalid controlled values

- **WHEN** an input supplies a `status` or `effort` value outside its declared set or with different capitalization
- **THEN** validation fails before persistence

#### Scenario: Reject missing or blank required values

- **WHEN** a create input omits any required writable field, or a create or update input supplies null, an empty string, or only whitespace for one
- **THEN** validation fails before persistence

#### Scenario: Validate optional JSON metadata

- **WHEN** an input supplies `metadata`
- **THEN** the system accepts it only when it is a valid JSON object

#### Scenario: Generate creation metadata

- **WHEN** an activity is created successfully
- **THEN** the system generates its `id` and its UTC `created_at`, and does not accept either value as caller-controlled data

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

The system SHALL apply the same activity field rules to every create and update input and SHALL validate all input before persistence. An update MAY omit writable fields to preserve their stored values, but it SHALL not set a required field to null, an empty string, or whitespace. An update SHALL allow `reference` or `metadata` to be explicitly set to null.

#### Scenario: Preserve generated fields during update

- **WHEN** an update attempts to provide or change `id` or `created_at`
- **THEN** validation fails and the stored generated fields remain unchanged

#### Scenario: Clear an optional field during update

- **WHEN** an update explicitly supplies `reference: null` or `metadata: null`
- **THEN** the system stores a null value for that optional field

#### Scenario: Validate before saving

- **WHEN** an add or update input is invalid
- **THEN** the system returns a validation result without executing the persistent write
