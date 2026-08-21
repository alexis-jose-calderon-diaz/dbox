# activity-data-portability Specification

## Purpose

Permitir transferir el catalogo de actividades entre proyectos mediante formatos estructurados, validables y estables para personas, scripts y agentes.

## Requirements

### Requirement: Export portable activity records

The system SHALL expose `dbox activity export` to write every activity from the resolved project database in `created_at ASC`, `id ASC` order. The command SHALL accept an optional `--format <json|jsonl>` option that defaults to `json`, SHALL apply pending migrations before reading, and SHALL export every public activity field, including `id`, `created_at`, `updated_at`, and `version`.

#### Scenario: Export the catalog as JSON

- **WHEN** a user runs `dbox activity export` in a project containing activities
- **THEN** stdout contains exactly one JSON array of complete activity records in `created_at ASC`, `id ASC` order

#### Scenario: Export an empty catalog as JSON

- **WHEN** a user runs `dbox activity export --format json` in an empty initialized project
- **THEN** stdout contains exactly the JSON array `[]`

#### Scenario: Export the catalog as JSONL

- **WHEN** a user runs `dbox activity export --format jsonl` in a project containing activities
- **THEN** stdout contains one complete activity record as a valid JSON object per line in `created_at ASC`, `id ASC` order and contains no auxiliary output

#### Scenario: Export an empty catalog as JSONL

- **WHEN** a user runs `dbox activity export --format jsonl` in an empty initialized project
- **THEN** stdout is empty and the command exits successfully

#### Scenario: Reject an unsupported export format

- **WHEN** a user supplies an export format other than `json` or `jsonl`
- **THEN** the command returns the JSON validation error defined by the CLI contract and does not write portable data

### Requirement: Atomically import exported activity records

The system SHALL expose `dbox activity import --file <path> --format <json|jsonl>` to import complete activity records. The command SHALL require both options, apply pending migrations before importing, preserve every supplied public field including `id`, `created_at`, `updated_at`, and `version`, and return exactly one JSON object with non-negative integer `imported` and the selected `format`. It SHALL insert the entire validated input in one transaction or persist none of it.

#### Scenario: Import a JSON export

- **WHEN** a user imports a readable JSON file containing an array of valid complete activity records with IDs absent from the target database
- **THEN** the command creates every record with its supplied public fields and returns its imported count and `format: "json"`

#### Scenario: Import a JSONL export

- **WHEN** a user imports a readable JSONL file containing one valid complete activity record per line with IDs absent from the target database
- **THEN** the command creates every record with its supplied public fields and returns its imported count and `format: "jsonl"`

#### Scenario: Reject malformed or incomplete portable data

- **WHEN** an import file does not match its selected JSON or JSONL structure, contains an unknown property, omits a public field, or contains a field value that violates the activity contract
- **THEN** the command returns a JSON validation error before starting a persistent write and imports no records

#### Scenario: Reject duplicate imported IDs

- **WHEN** an import file repeats an ID or supplies an ID already present in the target database
- **THEN** the command returns the conflict error defined by the CLI contract and imports no records

#### Scenario: Fail to read the import file

- **WHEN** the path supplied to `--file` cannot be opened or read
- **THEN** the command returns the I/O error defined by the CLI contract and imports no records

#### Scenario: Roll back a failed import write

- **WHEN** a database failure occurs while persisting a validated import
- **THEN** the command returns the database error defined by the CLI contract and the target database contains none of the imported records
