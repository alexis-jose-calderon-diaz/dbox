## Context

See `proposal.md` for the motivation. Today, project discovery is coupled to
database-backed commands, `activity schema` uses that path and migrates, and a
delete is immediately persistent. The change adds a read-only inspection
command, separates installed-contract discovery from database lifecycle, and
adds safety checks without adding a global database or changing the SQLite
schema.

## Goals / Non-Goals

**Goals:**
- Represent database discovery as `found`, `incomplete`, or `not_found` so the
  root `context` command and database-backed commands share the same boundary
  semantics.
- Keep schema discovery and dry-run deletion non-mutating.
- Make destructive deletion explicit and make Linux project artifacts private.
- Preserve the existing JSON output and error execution path for all commands
  outside the changed contracts.

**Non-Goals:**
- Add interactive prompts, recovery storage, global configuration, or a
  database-path override.
- Alter the activity data model, migrations, or schema JSON shape.
- Promise POSIX permission modes on non-Linux platforms.

## Decisions

### Return a tri-state location result from the central locator

The location component will return a result that always retains the normalized
absolute working directory and, when a `.dbox` boundary is found, the absolute
project directory, `.dbox` directory, and expected database path. Its state
will distinguish a present `data.db` (`found`) from a boundary without it
(`incomplete`) and no boundary (`not_found`). `context` maps this result
directly to its successful JSON response; database-backed commands retain their
current error mapping for every non-`found` state.

This avoids a second discovery walk that could disagree about nested projects or
incomplete boundaries. Returning only a nullable database path was rejected
because it cannot distinguish `incomplete` from `not_found` or report the
resolved boundary paths.

### Build schema output from installed metadata only

`activity schema` will render its existing JSON contract directly from the
shared activity metadata and bypass both the locator and database service. It
will therefore not execute migrations even when called beneath an initialized
project.

Reading the database only to preserve the previous migration behavior was
rejected because it contradicts the installed-CLI contract and makes schema
introspection unavailable before initialization.

### Make dry-run an alternate delete execution path

The delete command will parse `--yes` and `--dry-run` before invoking its
action. With neither option it returns the existing validation-error shape.
With `--dry-run`, it locates and reads the activity without calling migration or
save logic, returns the activity in the specified preview envelope, and wins
over `--yes` when both flags are present. With `--yes` alone, it follows the
normal migration, lookup, delete, and save path.

Treating `--yes --dry-run` as a syntax error was rejected because an accidental
or inherited confirmation flag must not turn a requested preview into a
destructive action. An interactive confirmation was rejected because it breaks
automation and is outside the decided scope.

### Apply private modes after initialization work on Linux

The initialization path will guard POSIX mode handling with the Linux runtime
check. It will ensure `.dbox` uses `0700`, then apply migrations, then set
`data.db` and any existing SQLite `-wal`, `-shm`, and `-journal` sidecars to
`0600`. It will perform the same normalization for an already initialized
project. A mode-setting failure will surface as the existing database error,
because reporting a successful private initialization without the requested
protection is unsafe.

Applying POSIX modes unconditionally was rejected because their semantics are
not portable. Relying solely on the private parent directory was rejected
because the contract explicitly protects the database and its known sidecars.

## Risks / Trade-offs

- A pending migration can leave `delete --dry-run` unable to query an older
  database shape → it returns the established database error rather than
  mutating the database, preserving dry-run semantics.
- Existing scripts that delete without confirmation will fail validation → the
  proposal marks this as breaking and migration guidance will require `--yes`
  for an intentional deletion.
- SQLite sidecars can be transient and disappear before permission normalization
  → process only files present after initialization; the private `.dbox`
  directory still restricts directory access on Linux.
- Linux permission APIs can fail on inaccessible existing paths → map the
  failure consistently to the existing initialization error contract without
  replacing or deleting data.

## Migration Plan

1. Release the CLI change with documentation and help that mark `delete --yes`
   as required and describe `--dry-run` and `context`.
2. Update automated consumers from `dbox activity delete <id>` to `dbox activity
   delete <id> --yes`; consumers that need inspection use `--dry-run`.
3. Existing projects require no data migration. Their next `dbox init` on Linux
   normalizes the permissions of the directory, database, and present sidecars.
4. Rollback is a CLI binary rollback only; it does not require changing the
   database. Any hardened modes remain private and compatible with prior CLI
   versions for the owning user.
