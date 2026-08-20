## MODIFIED Requirements

### Requirement: Schema discovery command

The system SHALL provide `dbox activity schema` and `dbox activity --schema` as equivalent commands that resolve the project database, apply pending migrations, and expose the public activity contract without exposing SQLite or EF Core internals.

#### Scenario: Request the human schema

- **WHEN** a user runs `dbox activity schema`
- **THEN** the system prints a readable description of the `activity` entity and its rules

#### Scenario: Request the JSON schema

- **WHEN** a user runs `dbox activity schema --json` or `dbox activity --schema --json`
- **THEN** the system prints the stable JSON contract under `entities.activity.fields` with no database introspection details or auxiliary text

#### Scenario: Use the schema alias

- **WHEN** a user runs `dbox activity --schema` or `dbox activity --schema --json`
- **THEN** the system behaves exactly as the corresponding `dbox activity schema` command
