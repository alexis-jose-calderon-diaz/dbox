# project-context Specification

## Purpose

Expose a stable, non-mutating JSON view of project discovery so callers can
inspect their dbox context before attempting database-backed operations.

## Requirements

### Requirement: Inspect project discovery context

The system SHALL expose `dbox context` as a root infrastructure command. The
command SHALL use the same upward `.dbox` discovery algorithm as database-backed
commands, SHALL NOT create, open, migrate, or otherwise modify a database, and
SHALL write exactly one JSON object to stdout with exit code `0` for every
discovery outcome. The object SHALL include `status`, absolute `cwd`, and the
absolute resolved `project_directory`, `dbox_directory`, and `database` paths.
For `not_found`, the three unresolved path properties SHALL be `null`.

#### Scenario: Report a discovered database

- **WHEN** `dbox context` runs from a directory whose nearest `.dbox` contains `data.db`
- **THEN** it returns `status: "found"`, the absolute working directory, and absolute paths for that project directory, `.dbox` directory, and `data.db`

#### Scenario: Report an incomplete project boundary

- **WHEN** `dbox context` finds a nearest `.dbox` directory without `data.db`
- **THEN** it returns `status: "incomplete"`, the absolute working directory, the boundary's absolute project and `.dbox` paths, and that boundary's absolute expected `data.db` path without searching higher ancestors

#### Scenario: Report no project context

- **WHEN** `dbox context` reaches the filesystem root without finding `.dbox`
- **THEN** it returns `status: "not_found"`, the absolute working directory, and `null` for `project_directory`, `dbox_directory`, and `database`

#### Scenario: Inspect context without a usable database

- **WHEN** `dbox context` runs where no database exists or where the nearest project is incomplete
- **THEN** it returns its context response rather than a database-not-found or database error
