## MODIFIED Requirements

### Requirement: Fixed activity fields and rules
The system SHALL expose exactly the fixed `activity` contract with immutable generated integer `id`; immutable generated UTC datetimes `created_at` and `updated_at`; immutable generated positive integer `version`; required, non-blank strings `type`, `title`, `description`, `status`, `source`, `area`, `result`, `impact`, and `effort`; and optional nullable `reference` and `metadata`. `updated_at` SHALL equal `created_at` on creation and change only after a successful update. `version` SHALL start at `1` and increment once after each successful update. `type`, `source`, and `area` SHALL be extensible strings. `status` SHALL be one of `pending`, `in_progress`, or `completed`; `effort` SHALL be one of `low`, `medium`, `high`, or `very-high`; `title` SHALL be at most 200 characters; and `metadata` SHALL be a nullable JSON object.

#### Scenario: Describe the activity contract
- **WHEN** a user requests the activity schema in JSON
- **THEN** the response identifies the `activity` entity and reports each fixed field with its public type, required status where applicable, generated status, mutability, enum values, maximum length, and default value

#### Scenario: Reject invalid controlled values
- **WHEN** an input supplies a `status` or `effort` value outside its declared set or with different capitalization
- **THEN** validation fails before persistence

#### Scenario: Reject invalid required values
- **WHEN** an input omits a required writable field or supplies null, an empty string, or whitespace for one
- **THEN** validation fails before persistence

#### Scenario: Reject an oversized title
- **WHEN** an input supplies a title longer than 200 characters
- **THEN** validation fails before persistence

#### Scenario: Generate creation metadata
- **WHEN** an activity is created successfully
- **THEN** the system generates its `id`, UTC `created_at`, UTC `updated_at`, and `version` equal to `1`, and does not accept any of them as caller-controlled create data

#### Scenario: Report modification metadata
- **WHEN** a user requests an existing activity through `get` or receives it from a successful create or update
- **THEN** the complete activity includes its immutable `updated_at` and `version` values

### Requirement: Consistent activity validation
The system SHALL apply the same activity field rules to every create, update, and import input and SHALL validate all input before persistence. `id`, `created_at`, `updated_at`, and `version` SHALL be generated and immutable in ordinary create and update data; an import record SHALL instead provide every public field exactly as exported and validate its generated fields as positive integer or UTC datetime values, as applicable.

#### Scenario: Preserve generated fields during update
- **WHEN** an update attempts to provide or change `id`, `created_at`, or `updated_at`
- **THEN** validation fails and the stored generated fields remain unchanged

#### Scenario: Validate before saving
- **WHEN** an add, update, or import input is invalid
- **THEN** the system returns a validation result without executing the persistent write

#### Scenario: Validate imported generated metadata
- **WHEN** an import record provides a non-positive `id` or `version`, or a `created_at` or `updated_at` value that is not a UTC datetime
- **THEN** validation fails before the import transaction begins
