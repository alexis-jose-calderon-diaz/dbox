## MODIFIED Requirements

### Requirement: Schema discovery command
The system SHALL provide only `dbox activity schema` to resolve the project
database, apply pending migrations, and expose the public activity contract.
The command SHALL return the stable JSON contract under
`entities.activity.fields` without SQLite or EF Core internals or auxiliary
text.

#### Scenario: Request the activity schema
- **WHEN** a user runs `dbox activity schema`
- **THEN** stdout contains the stable JSON activity contract under `entities.activity.fields`

#### Scenario: Reject schema format and alias options
- **WHEN** a user runs `dbox activity schema --json` or `dbox activity --schema`
- **THEN** the system returns the JSON command syntax validation error defined by the CLI contract
