## Context

See `proposal.md` for the motivation and the delta specifications for the observable contract. The current public model uses JSON payload options for activity commands, list filters are limited, and `list` returns a bare array. The project requires JSON-only operational output, stable validation errors, EF Core query composition, and no schema or migration change.

## Goals / Non-Goals

**Goals:**

- Keep a single parsing and validation path for inline JSON, files, and standard input.
- Apply one shared filter model to list and count so their matching totals remain consistent.
- Calculate pagination metadata against the complete filtered query while returning bounded result sets by default.
- Preserve database discovery, automatic migrations, JSON error handling, and exit-code behavior.

**Non-Goals:**

- Importing or exporting data.
- Altering the activity schema, stored data, or migrations.
- Adding arbitrary queries, sort selection, cursor pagination, or additional output formats.

## Decisions

### Represent input as a selected JSON source before parsing

Each JSON-consuming command will define `--json` and `--json-file` as exclusive options, select exactly one source, read it as UTF-8, and then pass its contents to the existing JSON object parser and command-specific validator. `--json-file -` will read standard input rather than a filesystem path.

This keeps object-shape, unknown-field, and field-value validation identical across sources, and maps source-selection, read, and parse failures into the existing validation-error pipeline. Duplicating file and stdin parsing in each command was rejected because it would let error behavior drift.

### Share a composable activity-filter query

A dedicated filter input will carry the nine permitted filter properties. One shared query builder will validate the complete filter and apply equality, inclusive UTC bounds, and partial-field predicates to an `IQueryable<Activity>` before either command obtains a count or list page.

`list` will count the filtered query before `Skip` and `Take`; `count` will only count that same filtered query. Separate list and count predicates were rejected because they risk inconsistent results for identical payloads.

Partial `title` and `description` matching will be case-insensitive. The query implementation will use an explicit provider-translatable comparison rather than relying on a database's default collation, so results do not vary across SQLite environments. Case-sensitive matching was rejected because it makes free-text lookup less useful for interactive and automated callers.

### Make pagination explicit in an envelope

`list` will use an effective `skip` of `0` and `take` of `100` when absent. It will reject `--all` combined with an explicitly supplied `--take`. With `--all`, it will omit the query limit and serialize `pagination.take` as `null`; otherwise it will serialize the effective integer take. `has_more` will be derived from the filtered total and requested page boundary, rather than by querying records beyond the page.

The bare array cannot carry the required total or navigation metadata. Fetching `take + 1` rows was rejected because it does not provide `total` and adds a second pagination rule.

### Validate options before database query execution

The command layer will validate source exclusivity, pagination values, date format and ordering, and filter shape before creating the data-operation query. All known input failures will use the existing structured `validation_error` response with exit code `2` and empty stdout.

Relying on parser, filesystem, or provider exceptions directly was rejected because their messages and types are not stable CLI behavior.

## Risks / Trade-offs

- [Counting large filtered catalogs adds a database operation to each list request] → The response contract requires `total`; use the same filtered query and avoid materializing all records unless `--all` is explicit.
- [Standard input can be unavailable or contain unexpected content in interactive invocations] → Treat read and parse failures as deterministic validation errors with no partial command operation.
- [Date-time parsing or text matching can vary with storage/provider collation] → Parse only strict UTC `Z` timestamps, use an explicit case-insensitive predicate, and add integration coverage for the documented partial-search behavior.
- [The list envelope breaks existing array consumers] → Mark the change breaking, document the replacement response, and require downstream callers to select `items`.

## Migration Plan

1. Release the contract as a breaking CLI version with help and schema-facing documentation updated alongside the command changes.
2. Update automated callers of `activity list` to consume `items` and `pagination`.
3. Deploy without a database migration because persisted activity data and schema are unchanged.
4. Roll back by restoring the preceding CLI version; existing databases remain compatible because no migration is introduced.
