## Why

El contrato actual de `activity` solo registra una clasificacion, titulo, descripcion y estado, lo que no alcanza para analizar el resultado, impacto y esfuerzo del trabajo de desarrollo. Antes del primer release publico, dbox debe fijar su esquema inicial completo sin mantener compatibilidad con un modelo que aun no es publico.

## What Changes

- **BREAKING** Ampliar la entidad fija `activity` con los campos obligatorios `source`, `area`, `result`, `impact` y `effort`, y con los campos opcionales `reference` y `metadata`.
- **BREAKING** Hacer obligatorio `description` y eliminar el valor predeterminado de `status`; todos los campos de negocio obligatorios deben recibirse al crear una actividad.
- **BREAKING** Reemplazar el enum cerrado de `type` por un string obligatorio extensible; `source` y `area` tambien seran strings obligatorios extensibles.
- Definir `effort` como el unico nuevo conjunto controlado: `low`, `medium`, `high` y `very-high`.
- Almacenar `metadata` como JSON opcional, validarlo como JSON estructurado y exponerlo como JSON en las respuestas de actividades.
- Actualizar `add`, `update`, `get`, `list` y `schema` para aceptar, validar y devolver el contrato completo; los filtros y conteos existentes continuaran funcionando con el `type` no enumerado.
- Regenerar la migracion inicial de EF Core para que la tabla `activities` represente directamente el contrato oficial, sin migracion ni logica de compatibilidad.

## Capabilities

### New Capabilities

Ninguna.

### Modified Capabilities

- `activity-contract`: redefine los campos fijos, obligatoriedad, valores controlados, metadata JSON y el documento de schema.
- `activity-crud`: redefine los payloads y respuestas de los comandos CRUD con todos los campos del nuevo contrato.
- `activity-count`: adapta la validacion de filtros compartidos al `type` extensible.

## Impact

- Afecta el modelo `Activity`, la metadata centralizada de `ActivitySchema`, parseres, validadores, vistas JSON, repositorio y comandos del catalogo `activity`.
- Afecta el mapeo EF Core, el snapshot y la migracion inicial de SQLite bajo `src/Dbox/Database/Migrations/`.
- Afecta `PROJECT.md`, las specs principales y pruebas de contrato, comandos e integracion.
- No agrega dependencias, tablas auxiliares, entidades dinamicas ni columnas dedicadas para commits, branches, issues, pull requests u OpenSpec.
