## 1. Bootstrap

- [x] 1.1 Create `Dbox.sln`, the .NET 10 executable project under `src/Dbox/`, and the xUnit project under `tests/Dbox.Tests/`.
- [x] 1.2 Add pinned, .NET 10-compatible dependencies for `System.CommandLine`, EF Core SQLite, design-time EF tooling, and xUnit.
- [x] 1.3 Configure the executable name as `dbox`, add the minimal `Program.cs` composition root, and make the test project reference the application.
- [x] 1.4 Add the root `.gitignore` entries for `bin/`, `obj/`, `.dbox/`, and temporary test artifacts.

## 2. Activity Contract

- [x] 2.1 Define the `activity` model and the shared `ActivitySchema` metadata for fields, generated values, mutability, enum values, defaults, and title length.
- [x] 2.2 Implement create and update input models that preserve omitted fields versus explicit nulls and reject unknown or read-only JSON properties.
- [x] 2.3 Implement activity validation from `ActivitySchema`, including case-sensitive enums, title rules, required fields, and validation before persistence.
- [x] 2.4 Implement the public activity representations and UTC `created_at` formatting required by text and JSON output.

## 3. Database Lifecycle

- [x] 3.1 Implement `DboxLocator` to normalize a starting directory, find the nearest `.dbox`, stop at incomplete boundaries, and return resolved database paths.
- [x] 3.2 Implement `DboxDbContext` with explicit `activities` mapping and a design-time factory that uses a non-persistent SQLite source.
- [x] 3.3 Generate and commit the initial EF Core migration with `dotnet-ef` without creating a user project database.
- [x] 3.4 Implement initialization status detection and migration execution without deleting, replacing, or truncating an existing database.
- [x] 3.5 Add the shared database execution path that discovers the project and applies migrations before every non-`init` operation.

## 4. CLI Runtime And Output

- [x] 4.1 Compose the root `System.CommandLine` command tree, global output selection, data command options, and the `schema`/`--schema` alias.
- [x] 4.2 Implement text and JSON writers so successful output is deterministic, goes only to `stdout`, and contains no migration diagnostics.
- [x] 4.3 Implement structured error mapping for validation, missing resources, missing databases, database failures, unexpected errors, `stderr`, and exit codes 1 through 4.
- [x] 4.4 Ensure parser and command errors follow the same text/JSON error contract instead of the default unstructured output.

## 5. Initialization And Schema

- [x] 5.1 Implement `init` against the current directory only, including initialized, already-initialized, migrated, and JSON responses.
- [x] 5.2 Implement `schema` and both `--schema` forms from `ActivitySchema`, with human-readable output and the stable JSON shape.

## 6. Activity Commands

- [x] 6.1 Implement the activity-specific repository operations with EF Core queries and no generic repository or handwritten CRUD SQL.
- [x] 6.2 Implement `add` with mutually exclusive options/JSON input, default status, validation, and complete activity responses.
- [x] 6.3 Implement `list` with `id DESC` ordering, combinable type/status filters, text table output, and JSON arrays including empty results.
- [x] 6.4 Implement `get` with complete activity responses and the documented resource-not-found behavior.
- [x] 6.5 Implement `update` with partial changes, explicit JSON null for description, generated-field protection, and empty-update validation.
- [x] 6.6 Implement `delete` without confirmation, including success responses and the same not-found behavior as `get`.

## 7. Verification

- [x] 7.1 Add locator tests for parent discovery, nearest-project precedence, nested initialization, filesystem-root termination, and incomplete `.dbox` boundaries.
- [x] 7.2 Add database tests for initial migration, repeated initialization, pending migrations, migration failures, and isolation between temporary projects.
- [x] 7.3 Add schema and validation tests for metadata, enum casing, title limits, defaults, generated fields, and pre-persistence rejection.
- [x] 7.4 Add CLI contract tests for text/JSON output, stdout/stderr separation, exact messages, error envelopes, aliases, and exit codes.
- [x] 7.5 Add CRUD integration tests for option and JSON input, ordering, filters, partial updates, description clearing, not-found cases, and deletion.
- [x] 7.6 Run compilation, the complete test suite, and the documented manual text/JSON acceptance flows from separate temporary directories.
