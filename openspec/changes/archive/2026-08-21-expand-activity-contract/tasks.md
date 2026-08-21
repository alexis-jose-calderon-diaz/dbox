## 1. Contract and Shared Activity Rules

- [x] 1.1 Update `PROJECT.md` with the complete initial `activity` table, required and optional fields, extensible categories, controlled `effort` values, JSON metadata rules, and revised `add`/`update` examples.
- [x] 1.2 Extend `ActivitySchema` and its schema-document types with all public fields, descriptions, required/generated/mutable metadata, and controlled values for `status` and `effort`.
- [x] 1.3 Update `Activity`, create/update inputs, JSON parsing, and validation so create requires every business field, updates preserve omitted fields, only optional fields accept null, and metadata accepts only JSON objects.

## 2. Persistence and Responses

- [x] 2.1 Update `DboxDbContext` to map every expanded field from `ActivitySchema`, with required SQLite columns for business fields and nullable columns only for `reference` and `metadata`.
- [x] 2.2 Regenerate the unpublished EF Core initial migration, designer, and model snapshot with `dotnet-ef` so a new database contains the complete official schema.
- [x] 2.3 Extend `ActivityView` to emit every field and parse stored metadata into a JSON object in command responses.

## 3. Activity Commands

- [x] 3.1 Update `dbox activity add` to construct complete activities with generated UTC `created_at` only after the new payload passes validation.
- [x] 3.2 Update repository and `dbox activity update` handling to apply each writable field, retain generated fields, and clear only `reference` or `metadata` when explicitly null.
- [x] 3.3 Update `get`, `list`, and `schema` responses to expose the expanded contract, retaining list order and pagination.
- [x] 3.4 Update shared list/count filter validation so non-blank extensible `type` values are accepted while `status` remains controlled.

## 4. Verification

- [x] 4.1 Update schema and command tests to assert field descriptions, required fields, controlled values, complete add/get/list responses, and generated identifiers and timestamps.
- [x] 4.2 Add validation tests for omitted or blank required fields, invalid `effort`, invalid metadata, generated-field rejection, and clearing optional fields during update.
- [x] 4.3 Update list/count tests to cover an extensible type filter and preserve ordering, pagination, and JSON error behavior.
- [x] 4.4 Run `dotnet build Dbox.sln` and `dotnet test Dbox.sln`.
