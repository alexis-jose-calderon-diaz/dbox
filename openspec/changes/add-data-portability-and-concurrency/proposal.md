## Why

Las actividades quedan aisladas en la base local y no existe una forma estable de transferirlas entre proyectos o conservar copias estructuradas. Ademas, una actualizacion concurrente puede sobrescribir cambios ya persistidos sin que el cliente lo detecte.

## What Changes

- Agregar exportacion e importacion de actividades con JSON como contrato principal y JSONL como formato alternativo inicial; CSV queda fuera de este cambio.
- Incorporar `updated_at` UTC y `version` entero generado por el sistema en el esquema, las respuestas de actividad y el contrato expuesto por `activity schema`.
- Exigir una version esperada en `activity update` y efectuar la escritura condicionalmente para rechazar actualizaciones obsoletas en lugar de perder cambios.
- Definir validacion, errores JSON, atomicidad de importacion y una nueva migracion de EF Core para los nuevos campos.

## Capabilities

### New Capabilities
- `activity-data-portability`: Importar y exportar actividades mediante contratos JSON y JSONL validados.

### Modified Capabilities
- `activity-contract`: Exponer y regir los nuevos campos generados `updated_at` y `version`.
- `activity-crud`: Actualizar actividades condicionalmente con una version esperada e incrementar su version y fecha de actualizacion.
- `cli-contract`: Exponer los comandos de portabilidad y el resultado de conflicto de concurrencia con salida JSON y exit code estables.

## Impact

Se modificaran el grupo de comandos `activity`, sus modelos, parser, validacion, vistas y persistencia EF Core, junto con una migracion EF Core nueva y pruebas aisladas de comandos e integracion. No se agregan dependencias externas ni acceso SQL CRUD manual.
