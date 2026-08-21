## MODIFIED Requirements

### Requirement: Root infrastructure and catalog hierarchy
The root `dbox` command SHALL expose only shared infrastructure operations and
available catalog groups. `init` and `context` SHALL be root infrastructure
operations, and `activity` SHALL be the available catalog group. Catalog-specific
operations SHALL NOT be exposed directly at the root.

#### Scenario: Discover root operations
- **WHEN** a user requests root help
- **THEN** the help presents `init`, `context`, and the `activity` catalog group, and does not present activity CRUD or schema commands as root commands

#### Scenario: Reject a removed flat activity command
- **WHEN** a user invokes `dbox schema`, `dbox add`, `dbox list`, `dbox get`, `dbox update`, or `dbox delete`
- **THEN** the system returns a command syntax validation error and does not perform an activity operation
