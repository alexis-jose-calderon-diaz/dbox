## ADDED Requirements

### Requirement: Root maintenance commands
The root `dbox` command SHALL expose `backup` and `doctor` as shared infrastructure operations. Catalog-specific operations SHALL remain outside the root.

#### Scenario: Discover maintenance operations
- **WHEN** a user requests root help
- **THEN** the help presents `backup` and `doctor` alongside the root operations and available catalog groups defined by the CLI contract

### Requirement: Maintenance command JSON responses
Successful root maintenance commands SHALL write exactly one JSON object to
stdout. `dbox backup` SHALL identify the created backup file, and `dbox doctor`
SHALL report database existence, opening status, SQLite integrity status,
pending known migrations, and inspected permissions. Neither command SHALL
write diagnostics or auxiliary text outside its JSON response.

#### Scenario: Return a backup result
- **WHEN** `dbox backup` successfully creates a backup
- **THEN** stdout contains exactly one JSON object that identifies the created backup file

#### Scenario: Return a doctor diagnostic result
- **WHEN** `dbox doctor` completes its read-only diagnostic checks
- **THEN** stdout contains exactly one JSON object with the documented maintenance diagnostic fields
