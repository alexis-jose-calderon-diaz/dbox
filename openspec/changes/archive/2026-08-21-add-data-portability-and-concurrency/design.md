## Context

See `proposal.md` for the motivation. The current activity contract exposes generated `id` and `created_at`, while ordinary updates accept only writable data and can overwrite a value read by another process. Catalog commands already discover the nearest project database and migrate it before operating; the implementation must retain that flow, use the small EF Core context and shared `ActivitySchema`, and keep all normal output machine-readable.

The delta specs introduce a portable representation that must round-trip complete activity views, new generated modification metadata, and a concurrency precondition for `update`. The JSONL export is the only intentional exception to the existing single-JSON-value stdout convention.

## Goals / Non-Goals

**Goals:**

- Define JSON as the default portable format and JSONL as an explicit streaming alternative.
- Preserve all public activity fields during an import/export round trip, including identity, timestamps, and version.
- Reject stale updates with a database-enforced expected-version condition.
- Make every import all-or-nothing and leave a migrated database valid.
- Keep validation and errors deterministic and compatible with the established JSON error envelope.

**Non-Goals:**

- CSV, Excel, format auto-detection, stdin import, filtering, paging, merge, upsert, or overwrite modes.
- Cross-project synchronization, conflict resolution UI, automatic retry, or bulk update concurrency.
- New third-party dependencies, raw SQLite connections or commands, handwritten CRUD SQL, generic repository layers, or changes to the project-location algorithm.

## Decisions

### Portable command and wire formats

`dbox activity export [--format json|jsonl]` reads the whole catalog in its existing stable order. `json` is the default and emits a single UTF-8 JSON array. `jsonl` emits one compact complete activity object per line, without a wrapper, blank lines, progress output, or summary. It is deliberately exposed only through `--format`; `--output` remains unsupported.

`dbox activity import --file <path> --format <json|jsonl>` reads UTF-8 data from the supplied path. JSON imports require one array; JSONL imports require one object on each non-empty input line, with blank lines rejected. The explicit format avoids ambiguous parsing and accidental acceptance of a file in a different contract. A pipe-based input API and automatic detection were considered, but they would add input-source and ambiguity rules without advancing the initial portability contract.

The portable record is the complete public `ActivityView`, not an add or update payload. It contains every public field, including generated `id`, `created_at`, `updated_at`, and `version`; import preserves those values exactly after validation. This permits lossless project transfer. IDs are therefore never remapped or merged: a duplicate ID inside the input or already in the target is a `conflict_error`. An append-only writable-field import was considered, but it would lose identifiers and generated history and would not round-trip an export.

### Shared contract and validation

Extend `Activity`, `ActivityView`, and the single `ActivitySchema` source of truth with `UpdatedAt` and `Version`. The schema command and every complete activity response derive their metadata and JSON representation from that definition. `updated_at` is emitted as a UTC ISO 8601 value with `Z`; `version` is an integer. New activities obtain one captured UTC instant for both timestamps and version `1`.

Keep ordinary add and update input types separate from the import record type. Add rejects every generated field. Update accepts `version` only as a required positive-integer precondition and rejects it as a writable value; it also rejects `id`, `created_at`, and `updated_at`. Import requires exactly the complete portable field set, validates generated IDs and versions as positive integers, validates timestamps as UTC values, then applies the existing field rules to each ordinary field. Parsing, unknown-property checks, shape checks, and all record validation finish before an import transaction or `SaveChangesAsync` begins.

This separation avoids weakening the public create/update contract merely to consume an export. It also lets `ActivitySchema` continue to be the one source for field rules while a narrowly scoped portable-record parser enforces the complete-record shape.

### Conditional update and conflict mapping

Configure `Version` as an EF Core concurrency token. For an update, load the requested activity to distinguish a missing ID from an existing one, validate the payload and expected version, set only supplied writable fields, assign a newly captured UTC `UpdatedAt`, and set the next version. Persist with EF Core using the original expected version as the concurrency value, so the generated update is conditional on both ID and version.

If the initial lookup finds no ID, return `resource_not_found`. If the conditional save affects no row or raises EF Core's concurrency exception, return `conflict_error` with exit code `3` and do not retry or overwrite the row. A concurrent deletion after a successful lookup is also treated as a conflict because the request's observed state is no longer current. This is preferable to a read-then-write check, which can still lose a race.

`updated_at` and `version` change only when the conditional update succeeds. The returned view is the persisted post-update entity, so callers can use its new version as the next expected version.

### Import transaction and failures

After the file has been read and fully parsed and validated, begin an EF Core transaction at serializable isolation. Within it, query the target IDs, reject any collision before adding entities, add all validated records with their preserved values, save once, and commit. Any validation issue occurs before the transaction; any conflict, provider constraint failure, or persistence exception rolls the transaction back. A provider-detected unique-key collision that races the preflight query is mapped to `conflict_error`; other persistence and migration failures remain `database_error`.

An unreadable path is `io_error` with exit code `4`; malformed data, missing required options, unsupported format, and invalid record values are `validation_error` with exit code `2`. No error writes portable data or a success response to stdout. A per-row transaction or partial-success response was rejected because it leaves a target catalog in an indeterminate transferred state.

Export applies pending migrations first, executes one ordered read, materializes the result before serializing, and does not start a write transaction. It returns database discovery and migration failures through the existing error path.

### EF Core migration and existing databases

Update the EF Core model configuration to make `updated_at` and `version` required, configure `version` as the concurrency token, and retain explicit activity table and column names. Generate a new migration with `dotnet ef` after the model change; do not edit an already-applied migration. The new migration adds both columns with provider-supported defaults so existing rows receive a positive version of `1` and a UTC migration-time `updated_at`. New application-created rows explicitly set both timestamps and their initial version, so they do not rely on the migration default.

The migration snapshot is regenerated by EF Core with the migration. `Database.MigrateAsync()` remains before every catalog operation, therefore preexisting project databases are upgraded before schema, export, import, or concurrency-aware CRUD executes. Rollback is the normal EF Core downgrade to the prior migration before deploying a version that lacks these columns; it must only be used when the operator accepts losing the new metadata and must never delete or replace the project database.

## Risks / Trade-offs

- [JSONL is not a single JSON value] -> Restrict the exception to `activity export --format jsonl`, document it in the CLI delta, and emit no auxiliary text.
- [Preserved IDs prevent importing into a catalog with overlapping IDs] -> Fail atomically with `conflict_error`; merge and remapping stay out of scope.
- [Timestamps can have the same observable precision on rapid updates] -> Version, not timestamp comparison, is the concurrency authority; each successful update increments it exactly once.
- [A competing importer can race the preflight ID query] -> Use a serializable transaction and map a final provider unique-key collision to the same deterministic conflict error.
- [Migrated activities did not have historical update metadata] -> Set their `updated_at` to the UTC migration time and their version to `1`; subsequent writes use normal generated values.
- [Large exports are materialized before serialization] -> The first supported contract favors one consistent ordered result and simple JSON/JSONL serialization; streaming database reads can be evaluated in a later change if catalog size requires it.
