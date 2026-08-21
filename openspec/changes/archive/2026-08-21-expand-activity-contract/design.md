## Context

The current `Activity` model, its EF Core mapping, input models, validation, JSON view, and initial migration only cover `type`, `title`, optional `description`, and `status`. `ActivitySchema` is already the shared source for public field metadata, validation rules, EF configuration, and the `activity schema` response. See `proposal.md` for the motivation and the delta specs for the externally observable behavior.

There is no public release or supported persisted database schema. The committed initial migration can therefore be regenerated to define the official initial table instead of adding a compatibility migration.

## Goals / Non-Goals

**Goals:**

- Preserve one fixed, small `activities` table that fully records development activity context and outcomes.
- Keep required-field, enum, schema, EF mapping, and CLI validation rules centralized in `ActivitySchema`.
- Store optional metadata as structured JSON while keeping SQLite persistence and EF Core mapping simple.
- Preserve the current `dbox activity` command hierarchy, JSON envelopes, ordering, pagination, and error conventions.

**Non-Goals:**

- Add user-defined columns, metadata indexing, metadata querying, or filters beyond the existing `type` and `status` filters.
- Add special columns or relationships for OpenSpec, commits, branches, issues, or pull requests.
- Support upgrading databases initialized with the prior unpublished schema.
- Add new lifecycle timestamps or change the existing generated `id` and `created_at` behavior.

## Decisions

### Expand the fixed model and preserve snake_case public names

`Activity` will add `Source`, `Area`, `Result`, `Impact`, `Effort`, `Reference`, and `Metadata`. The first five are non-nullable required properties; `Reference` and `Metadata` are nullable. The existing `Description` property becomes required. Public JSON and SQLite names stay in snake_case: `source`, `area`, `result`, `impact`, `effort`, `reference`, and `metadata`.

This preserves the current CLR naming and public serialization convention without introducing a second transport model for persistence.

### Centralize field definitions and validate semantic categories

`ActivitySchema` will define all thirteen public fields, including descriptions and the values permitted by `status` and `effort`. `type`, `source`, and `area` will be required non-blank strings with no enum restriction. `status` keeps its existing controlled values; `effort` adds `low`, `medium`, `high`, and `very-high`; `status` no longer has a creation default.

The validator will use the shared definitions to require every writable business field on create, validate supplied required fields on update, and reject null, empty, or whitespace values. Title retains its 200-character limit. The filter parser will require a non-blank string when a `type` filter is supplied but will not validate it against a fixed list.

Using closed enums for `type`, `source`, or `area` was rejected because activity classifications and project areas must evolve without a CLI release. Using the same unconstrained approach for `effort` was rejected because reporting needs a stable qualitative scale.

### Persist metadata as JSON text and emit it as a JSON object

The SQLite column will be nullable `TEXT`; the entity stores the validated raw JSON object text. The JSON input parser will distinguish an omitted field, explicit `null`, a JSON object, and every invalid value. When returning an activity, the view will parse stored metadata into a cloned JSON element so the output contains an object rather than a quoted JSON string.

This avoids an EF Core value converter or a second table while retaining structured JSON in the public contract. Storing arbitrary JSON scalars or arrays was rejected: `metadata` is defined as an extensible object of named supplemental data, and an object shape keeps it distinct from the primary fields.

### Recreate the unpublished initial migration

The `DboxDbContext` mapping will configure the new columns from `ActivitySchema`, with required SQLite columns for all mandatory business fields and nullable columns only for `reference` and `metadata`. The existing initial migration, designer, and model snapshot will be regenerated with `dotnet-ef` after the model is updated.

Adding a second migration was rejected because an existing database that has recorded the old initial migration would need upgrade logic, which is explicitly out of scope before the first release.

### Keep activity command surfaces narrow

`add` receives all required writable fields in `--json`; `update` supports any writable field and may clear only the optional fields with JSON null. `get` and `list` use the expanded view. `schema` exposes the complete definitions, including human-readable descriptions. `count` retains only `type` and `status` filters, with `type` now extensible.

Adding `source`, `area`, or metadata filters is deferred because the requested model aims to support future reports, not become project-management search functionality.

## Risks / Trade-offs

- [A locally initialized development database records the old initial migration and will not match the regenerated migration] -> Treat it as unsupported pre-release data and reinitialize it rather than adding compatibility code.
- [Metadata text can become invalid through data modified outside dbox] -> Validate it before persistence and surface a database error rather than silently emitting malformed JSON.
- [Schema metadata, validation, and EF mapping could drift as fields are added] -> Keep all field properties in `ActivitySchema` and cover the full schema and CRUD payload in tests.
- [Large metadata objects can make activity responses larger] -> Keep metadata optional and do not add indexing or report-query behavior in this change.

## Migration Plan

1. Update the main project contract and the activity rules/model to the full fixed field set.
2. Replace the committed unpublished initial migration and snapshot by generating them with `dotnet-ef` from the updated model.
3. Reinitialize any local development `.dbox/data.db` created from the old initial migration; no runtime conversion or fallback is provided.
4. Update command and integration tests to create complete activities, assert validation and JSON metadata behavior, then run the documented build and full test suite.

Rollback before release consists of reverting the source and regenerated initial migration together, then reinitializing development databases. There is no persisted public database to preserve.
