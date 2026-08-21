## Why

Los consumidores automatizados necesitan recorrer catálogos grandes, expresar consultas de actividad más útiles y suministrar payloads JSON sin depender de límites prácticos de la línea de comandos. El contrato actual devuelve listas sin metadatos de paginación y limita los filtros a `type` y `status`.

## What Changes

- **BREAKING** Cambiar `dbox activity list` para que devuelva el envelope JSON `{ "items": [...], "pagination": { "skip", "take", "total", "has_more" } }` en lugar de un arreglo JSON.
- Establecer `take` predeterminado en `100`, conservar `--skip`, añadir `--all` para eliminar el límite y rechazar su uso junto con `--take`.
- Ampliar los filtros JSON de `activity list` y `activity count` con `area`, `source`, `effort`, el rango UTC `created_from`/`created_to` y búsqueda parcial en `title`/`description`, manteniendo `type` y `status`.
- Permitir `--json-file <path>` y `--json-file -` para leer JSON desde stdin como alternativa mutuamente exclusiva de `--json` en los comandos que aceptan payloads JSON.
- Definir validaciones, errores JSON y mensajes deterministas para paginación, filtros y fuentes de entrada incompatibles.
- Excluir importación y exportación; se abordarán en un cambio separado.

## Capabilities

### New Capabilities

Ninguna.

### Modified Capabilities

- `activity-crud`: Cambiar la respuesta, paginación y filtros de `activity list`, y admitir JSON desde archivo o stdin en `add`, `list` y `update`.
- `activity-count`: Ampliar los filtros compartidos y admitir JSON desde archivo o stdin.
- `cli-contract`: Documentar las opciones de consulta y el contrato de validación para las fuentes de JSON excluyentes.

## Impact

Se modificarán los comandos `activity add`, `list`, `count` y `update`, el análisis y la validación de payloads y filtros, la consulta EF Core y sus pruebas. No hay cambios de esquema, migraciones, importación, exportación ni dependencias nuevas previstas.
