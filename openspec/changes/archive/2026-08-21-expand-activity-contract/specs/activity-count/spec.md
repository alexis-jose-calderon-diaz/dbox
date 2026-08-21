## MODIFIED Requirements

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
