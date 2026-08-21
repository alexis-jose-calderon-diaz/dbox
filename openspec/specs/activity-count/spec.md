# activity-count Specification

## Purpose

Provide a stable JSON count of all activities or of those matching the same
optional filters available to activity listing.

## Requirements

### Requirement: Count activities

The system SHALL expose `dbox activity count` and write a JSON object with a
non-negative integer `count` property to stdout. The command SHALL count all
activities when no filter payload is supplied.

#### Scenario: Count all activities

- **WHEN** a user runs `dbox activity count` in an initialized project with three activities
- **THEN** stdout contains exactly one JSON object with `count` equal to `3`

#### Scenario: Count an empty catalog

- **WHEN** a user runs `dbox activity count` in an initialized project with no activities
- **THEN** stdout contains exactly one JSON object with `count` equal to `0`

### Requirement: Filter an activity count

The system SHALL accept an optional filter object on `dbox activity count` from exactly one of `--json <object>` or `--json-file <path>`; `--json-file -` SHALL read the object from standard input. The filter object SHALL accept only `type`, `status`, `area`, `source`, `effort`, `created_from`, `created_to`, `title`, and `description`, with the same validation and matching semantics as `dbox activity list`. The command SHALL apply every supplied filter and SHALL not paginate its result.

#### Scenario: Count matching filtered activities

- **WHEN** a user runs `dbox activity count --json '{"type":"research","status":"pending","area":"backend"}'`
- **THEN** the command returns the number of activities matching every supplied filter

#### Scenario: Count filters supplied from standard input

- **WHEN** a user pipes a valid filter object to `dbox activity count --json-file -`
- **THEN** the command returns the count of activities matching that filter

#### Scenario: Reject an invalid count filter

- **WHEN** the count payload contains an unknown property, an invalid enum value, invalid UTC range value, `created_from` later than `created_to`, or an invalid partial-search value
- **THEN** the command returns the JSON validation error defined by the CLI contract and does not return a count

#### Scenario: Reject conflicting count payload sources

- **WHEN** a user supplies both `--json` and `--json-file` to `dbox activity count`
- **THEN** the command returns the JSON validation error defined by the CLI contract and does not return a count
