## Why

`dbox` actualmente expone el catalogo `activity` directamente en la raiz, lo que hace que la CLI parezca especifica de actividades y no deja una estructura clara para catalogos locales futuros. Se necesita una jerarquia estable en la que una unica base por proyecto sostenga multiples areas de datos.

## What Changes

- Se establece que la raiz de `dbox` contiene exclusivamente operaciones de infraestructura compartida.
- Se conserva `dbox init` como el inicializador unico de `.dbox/data.db`; aplicara todas las migraciones instaladas para cualquier catalogo.
- **BREAKING** Se trasladan `schema`, `--schema`, `add`, `list`, `get`, `update` y `delete` al grupo `dbox activity`.
- **BREAKING** Se retiran las rutas planas equivalentes (`dbox schema`, `dbox add`, `dbox list`, `dbox get`, `dbox update` y `dbox delete`) sin aliases de compatibilidad.
- Se redefine la ayuda raiz para presentar la infraestructura compartida y las areas disponibles, inicialmente `activity`.
- Se actualizan contrato, documentacion y pruebas para reflejar que `activity` es el primer catalogo de una CLI extensible, preparada para areas futuras como `command` y `skill` sin crearlas aun.

## Capabilities

### New Capabilities

Ninguna.

### Modified Capabilities

- `project-database`: define `init` como infraestructura compartida para todos los catalogos del proyecto.
- `activity-crud`: mueve las operaciones CRUD observables de actividades bajo `dbox activity`.
- `activity-contract`: mueve el descubrimiento de schema y su alias al grupo `activity`.
- `cli-contract`: define la jerarquia raiz-area, la ayuda asociada y el retiro de rutas planas.

## Impact

- Afecta `PROJECT.md`, la composicion de `System.CommandLine` en `DboxCli`, normalizacion de aliases y mensajes de ayuda y validacion.
- Afecta todas las pruebas de CLI que invocan comandos de actividad.
- No modifica la ubicacion, descubrimiento, esquema ni migracion actual de `.dbox/data.db`.
- No agrega dependencias, tablas ni los catalogos `command` o `skill`.
