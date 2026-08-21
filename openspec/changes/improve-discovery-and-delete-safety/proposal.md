## Why

La deteccion actual de la base oculta el estado de descubrimiento y obliga a
consultar y migrar una base para conocer el contrato de `activity`. Ademas,
una eliminacion accidental no requiere una confirmacion explicita y los
permisos de los archivos locales no se endurecen en Linux.

## What Changes

- Hacer que `dbox activity schema` exponga el contrato de la CLI instalada sin
  resolver, abrir ni migrar `.dbox/data.db`.
- Agregar `dbox context`, una consulta de raiz que siempre devuelve JSON y
  comunica el directorio actual, las rutas resueltas y los estados `found`,
  `incomplete` o `not_found`.
- **BREAKING** Exigir `--yes` para que `dbox activity delete <id>` persista la
  eliminacion; agregar `--dry-run` para validar y previsualizar sin persistir,
  sin requerir `--yes` y sin confirmacion interactiva ni papelera.
- Endurecer en Linux los permisos creados por `init`: `.dbox` con `0700`,
  `data.db` con `0600` y los artefactos SQLite derivados con permisos privados
  equivalentes. Otros sistemas operativos no declaran modos POSIX.

## Capabilities

### New Capabilities
- `project-context`: Consulta JSON no mutante para inspeccionar el resultado del descubrimiento de proyecto.

### Modified Capabilities
- `activity-contract`: El esquema de `activity` pasa a describir el contrato de la CLI instalada sin usar una base de proyecto.
- `activity-crud`: La eliminacion requiere una confirmacion explicita y permite una previsualizacion sin persistencia.
- `cli-contract`: La raiz incorpora `context` y define su respuesta JSON exitosa para todos los resultados de descubrimiento.
- `project-database`: El descubrimiento se expone mediante `context` y la inicializacion protege los artefactos locales en Linux.

## Impact

Se modificaran los comandos raiz, schema, delete y el componente centralizado
de ubicacion de proyecto, junto con las pruebas de CLI, descubrimiento,
eliminacion e inicializacion. No se introducen dependencias ni cambios de
esquema o migraciones de EF Core.
