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

The system SHALL accept an optional `--json <object>` payload on `dbox activity count`. The payload SHALL accept only non-blank string `type` and valid `status` filters, apply both when provided, and validate them with the activity contract. `type` SHALL be matched exactly without restricting it to a closed set of values.

#### Scenario: Count matching filtered activities

- **WHEN** a user runs `dbox activity count --json` with a non-blank type and a valid status
- **THEN** the command returns the number of activities matching both filters

#### Scenario: Count an extensible type

- **WHEN** a user runs `dbox activity count --json` with a non-blank type that is not one of the historic type values
- **THEN** the command accepts the filter and returns the number of matching activities

#### Scenario: Reject an invalid count filter

- **WHEN** the count payload contains an unknown property, a non-string or blank `type`, or an invalid `status`
- **THEN** the command returns the JSON validation error defined by the CLI contract and does not return a count
