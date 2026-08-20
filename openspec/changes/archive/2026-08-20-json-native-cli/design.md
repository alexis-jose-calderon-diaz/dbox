## Context

See proposal.md for motivation. The current command tree exposes text and JSON
output modes, an activity schema alias normalized before parsing, separate
field options and JSON bodies for writes, and list filtering as command
options. Output formatting, error formatting, input parsing, commands, and
the activity repository must change together while database location and the
SQLite schema remain unchanged.

## Goals / Non-Goals

**Goals:**
- Define one JSON response path for all operational success and error cases.
- Make JSON bodies the only create and update input representation.
- Support optional structured filters, deterministic paging, and counts.
- Preserve native CLI help and positional IDs where they improve discoverability.

**Non-Goals:**
- Add bulk or JSONL operations, stdin payload transport, cursor pagination, or
  a database schema migration.
- Make the two-command count-plus-list workflow transactional across processes.

## Decisions

### Unconditional operational JSON with native help exception

`CommandExecutor` will select JSON without consulting an output option, and
`OutputWriter` will serialize all successful DTOs and `CliError` envelopes as
JSON. The command parser will retain its normal help path so `--help` remains
human-readable and avoids database access. Parser failures outside help are
converted to the same JSON error envelope before command invocation.

This removes `OutputFormat`, output parsing, text writers, forced schema JSON,
and the special error-format detector. Retaining `--output` as a compatibility
alias was rejected because the goal is to reduce the public surface and it
would preserve an ambiguous, redundant interface.

### Command-specific JSON payloads

`add` and `update` expose required `--json` string options. Their parsers read
exactly one JSON object and reject missing, non-object, unknown, generated, or
invalid fields. Field options are removed. `update` retains its positional ID,
which is deliberately excluded from its payload.

`list` and `count` expose an optional `--json` filter object. Omission is
represented internally as no filters rather than parsing an artificial `{}`.
Both commands share a small filter parser/validation path so accepted fields
and enum validation cannot drift. `init`, `schema`, `get`, and `delete` do not
declare a JSON payload option.

Using stdin was rejected because explicit command-line payloads are easier to
discover with native help and do not require terminal input-state handling.

### Ordered pagination and counting at repository boundary

The activity repository will build the same filtered EF Core query for list and
count. Listing orders by `CreatedAt`, then `Id`, before `Skip` and optional
`Take`; counting executes the filtered query without pagination. A dedicated
count response DTO keeps the public response shape stable.

Offset pagination was selected because the requested interface is `--skip` and
`--take`. Cursor pagination was rejected because it would add cursor encoding
and a new public protocol before the catalog has demonstrated that need.

### Contract and test migration first

`PROJECT.md` and test expectations will move to the new public contract before
the command implementation changes. Tests will cover JSON-only success and
errors, native help, removed options and alias rejection, required and optional
payload behavior, count filters, order ties, and pagination boundaries.

## Risks / Trade-offs

- [Scripts using text output or field options break] -> Document every removed
  form and its JSON replacement in `PROJECT.md` and contract tests.
- [A malformed JSON argument is difficult to quote in shells] -> Keep payloads
  as a single `--json` option and expose field rules through `activity schema`.
- [Records change between `count` and a later paginated `list`] -> Each command
  returns its own consistent query result; callers that need a stable snapshot
  must tolerate concurrent local changes because cross-process snapshots are
  out of scope.
- [Same-timestamp activities could page unpredictably] -> Include `id ASC` as
  a mandatory secondary ordering key.

## Migration Plan

1. Update the project contract and the main behavioral specs through this
   change's delta specs.
2. Replace the command surface, parser, output writer, and repository query
   behavior in one implementation change.
3. Replace tests for the retired text and field-option interface with tests for
   the JSON-native commands, help, count, and pagination.
4. Build and run the complete test suite before archiving the change.

Rollback is a source rollback before release. No persisted data migration is
required because the SQLite schema and stored activity fields do not change.
