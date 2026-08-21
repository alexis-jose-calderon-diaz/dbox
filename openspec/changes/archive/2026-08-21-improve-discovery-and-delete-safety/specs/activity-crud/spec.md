## MODIFIED Requirements

### Requirement: Delete an activity
The system SHALL delete an existing activity only through `dbox activity delete
<id> --yes`; it SHALL NOT prompt interactively or move data to a recycle bin.
The `--yes` option SHALL be required for a persistent deletion. The command
SHALL accept `--dry-run` as a non-mutating alternative that does not require
`--yes`, resolves and validates the existing activity without applying
migrations, and returns a preview object with the complete `activity`, its `id`,
`deleted: false`, and `dry_run: true`. When both options are supplied,
`--dry-run` SHALL take precedence and no deletion SHALL occur.

#### Scenario: Delete an existing activity after explicit confirmation
- **WHEN** a user runs `dbox activity delete <id> --yes` for an existing activity
- **THEN** the system removes the activity and returns its id with `deleted: true` in JSON output

#### Scenario: Reject an unconfirmed deletion
- **WHEN** a user runs `dbox activity delete <id>` without `--yes` or `--dry-run`
- **THEN** the system returns the JSON validation error defined by the CLI contract and does not persist a change

#### Scenario: Preview an existing deletion
- **WHEN** a user runs `dbox activity delete <id> --dry-run` for an existing activity
- **THEN** the system returns the complete activity with its id, `deleted: false`, and `dry_run: true` without deleting, migrating, or otherwise persisting a change

#### Scenario: Preview a missing deletion
- **WHEN** a user runs `dbox activity delete <id> --dry-run` for an id that does not exist
- **THEN** the system returns the same resource-not-found result as `get` without persisting a change

#### Scenario: Prefer a dry run when confirmation is also supplied
- **WHEN** a user runs `dbox activity delete <id> --yes --dry-run`
- **THEN** the system performs the non-mutating preview and does not delete the activity

#### Scenario: Delete a missing activity
- **WHEN** a user runs `dbox activity delete <id> --yes` for an id that does not exist
- **THEN** the system returns the same resource-not-found result as `get`
