## Purpose

Provide a stable JSON count of all activities or of those matching the same
optional filters available to activity listing.

## ADDED Requirements

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
The system SHALL accept an optional `--json <object>` payload on `dbox activity
count`. The payload SHALL accept only `type` and `status` filters, apply both
when provided, and validate their values with the activity contract.

#### Scenario: Count matching filtered activities
- **WHEN** a user runs `dbox activity count --json '{"type":"research","status":"pending"}'`
- **THEN** the command returns the number of activities matching both filters

#### Scenario: Reject an invalid count filter
- **WHEN** the count payload contains an unknown property or an invalid type or status value
- **THEN** the command returns the JSON validation error defined by the CLI contract and does not return a count
