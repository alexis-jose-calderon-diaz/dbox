## MODIFIED Requirements

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
