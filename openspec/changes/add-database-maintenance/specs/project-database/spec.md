## ADDED Requirements

### Requirement: Root maintenance operations avoid migration
Root maintenance commands SHALL resolve the same project database as catalog commands without applying migrations. `dbox backup` and `dbox doctor` SHALL not call the catalog data-operation migration path.

#### Scenario: Run a maintenance command against a database with pending migrations
- **WHEN** `dbox backup` or `dbox doctor` resolves a database that has pending known migrations
- **THEN** the command does not apply migrations before or after its maintenance operation
