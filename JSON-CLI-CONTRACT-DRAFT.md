# Borrador del contrato JSON de dbox

Este documento describe una posible CLI de `dbox` despues de eliminar los
aliases y las opciones de formato redundantes. No es todavia el contrato
oficial del proyecto.

La propuesta usa los nombres de los comandos como selectores textuales y usa
JSON como formato de salida y de los payloads:

```text
comando + --json '{...}' -> comando -> JSON por stdout
                                     -> JSON por stderr si hay un error
```

## Reglas generales

- Toda respuesta exitosa se escribe en `stdout` como un unico valor JSON.
- Todo error esperado se escribe en `stderr` como un unico objeto JSON.
- `--json '<objeto>'` y `--json-file <path>` son fuentes JSON alternativas y
  mutuamente excluyentes.
- `--json-file -` lee la fuente JSON desde `stdin`; los archivos se leen como
  UTF-8.
- Una fuente JSON es obligatoria en `add` y `update` y opcional en `list` y
  `count`; si se omite en estos ultimos, no hay filtros.
- No existen `--output text` ni `--output json`.
- No existen aliases de comandos.
- `add`, `list`, `count` y `update` reciben su payload mediante `--json` o
  `--json-file`.
- `get`, `update` y `delete` reciben el identificador como argumento
  posicional: `get 1`, `update 1` y `delete 1`.
- `init`, `context`, `backup`, `doctor` y `activity schema` no reciben payload.
- `activity delete <id> --yes` es la forma persistente y requiere confirmacion
  explicita.
- `activity delete <id> --dry-run` previsualiza sin migrar ni persistir; si se
  combinan `--dry-run` y `--yes`, prevalece `--dry-run`.
- La CLI no lee payloads desde `stdin` de forma implicita; solo lo hace cuando
  se especifica `--json-file -`.
- No se aceptan arreglos JSON ni propiedades desconocidas en los payloads.
- `--help` conserva la ayuda textual natural de una herramienta CLI.
- `list` ordena siempre por `created_at ASC` y usa `id ASC` como desempate.
- Los comandos conservan los codigos de salida actuales.

## Resumen de comandos

| Comando | Entrada | Respuesta exitosa |
| --- | --- | --- |
| `dbox init` | Ninguna | Objeto de inicializacion |
| `dbox context` | Ninguna | Contexto de descubrimiento |
| `dbox backup` | Ninguna | Referencia al backup creado |
| `dbox doctor` | Ninguna | Diagnostico read-only de la base |
| `dbox activity schema` | Ninguna | Documento de schema |
| `dbox activity add --json '{...}'` o `--json-file <path>` | Objeto de actividad | Actividad creada |
| `dbox activity list [--json '{...}' o --json-file <path>]` | Filtros opcionales | Envelope con actividades y paginacion |
| `dbox activity count [--json '{...}' o --json-file <path>]` | Filtros opcionales | Objeto con `count` |
| `dbox activity get 15` | ID posicional | Actividad solicitada |
| `dbox activity update 15 --json '{...}'` o `--json-file <path>` | Campos modificables | Actividad actualizada |
| `dbox activity delete 15 --yes` | ID y confirmacion | Confirmacion de borrado |
| `dbox activity delete 15 --dry-run` | ID y previsualizacion | Actividad a eliminar |

## Ayuda

La ayuda conserva el formato textual natural de `System.CommandLine` o de una
herramienta CLI convencional. La ayuda no forma parte del payload JSON de las
operaciones.

### `dbox --help`

```bash
dbox --help
```

Salida:

```text
Description:
  Local project catalog database CLI.

Usage:
  dbox [command] [options]

Commands:
  init                 Initialize the database in the current directory.
  context              Show the discovered project context.
  backup               Create a consistent backup of the project database.
  doctor               Diagnose the project database without modifying it.
  activity             Manage the activity catalog.

Options:
  --help               Show command line help.
```

### `dbox backup --help`

```bash
dbox backup --help
```

Salida:

```text
Description:
  Create a consistent backup of the project database.

Usage:
  dbox backup [options]

Options:
  --help               Show command line help.
```

### `dbox doctor --help`

```bash
dbox doctor --help
```

Salida:

```text
Description:
  Diagnose the project database without modifying it.

Usage:
  dbox doctor [options]

Options:
  --help               Show command line help.
```

### `dbox context --help`

```bash
dbox context --help
```

Salida:

```text
Description:
  Show the discovered project context.

Usage:
  dbox context [options]

Options:
  --help               Show command line help.
```

### `dbox activity --help`

```bash
dbox activity --help
```

Salida:

```text
Description:
  Manage the activity catalog.

Usage:
  dbox activity [command] [options]

Commands:
  schema               Show the public activity contract.
  add                  Create an activity.
  list                 List activities.
  count                Count activities.
  get                  Get one activity by ID.
  update               Update an activity by ID.
  delete               Delete an activity by ID.

Options:
  --help               Show command line help.
```

### Ayuda de un comando hoja

```bash
dbox activity add --help
```

Salida ilustrativa:

```text
Description:
  Create an activity.

Usage:
  dbox activity add [options]

Options:
  --json <json>            Activity input as a JSON object.
  --json-file <json-file>  Read activity input from a UTF-8 JSON file, or '-' for standard input.
  --help                   Show command line help.
```

`count --help` y `update --help` tambien muestran `--json-file` como alternativa
a `--json`. `list --help` muestra `--json-file`, `--skip`, `--take` y `--all`.
`get`, `update` y `delete` muestran que el ID es un argumento posicional.
`delete --help` tambien muestra `--yes` y `--dry-run`. Los detalles de campos y
enums se descubren con `dbox activity schema`.

Ejemplo de ayuda de `list`:

```bash
dbox activity list --help
```

```text
Description:
  List activities.

Usage:
  dbox activity list [options]

Options:
  --json <json>            Filters as a JSON object: type, status, area, source, effort, created_from, created_to, title, description.
  --json-file <json-file>  Read filters from a UTF-8 JSON file, or '-' for standard input.
  --skip <skip>            Number of ordered records to skip. Defaults to 0.
  --take <take>            Maximum number of records to return. Defaults to 100.
  --all                    Return all matching records without a limit.
  --help                   Show command line help.
```

Ejemplo de ayuda de `delete`:

```bash
dbox activity delete --help
```

```text
Description:
  Delete an activity.

Usage:
  dbox activity delete <id> [options]

Arguments:
  <id>                  Activity id.

Options:
  --yes                 Confirm the permanent deletion.
  --dry-run             Preview the deletion without changing the database.
  --help                Show command line help.
```

## Inicializacion

### `dbox init`

`init` opera sobre el directorio actual y no recibe payload.

```bash
dbox init
```

Base nueva:

```json
{
  "database": ".dbox/data.db",
  "status": "initialized"
}
```

Base ya actualizada:

```json
{
  "database": ".dbox/data.db",
  "status": "already_initialized"
}
```

Base existente con migraciones pendientes:

```json
{
  "database": ".dbox/data.db",
  "status": "migrated"
}
```

## Contexto del proyecto

### `dbox context`

`context` inspecciona el resultado del descubrimiento sin crear, abrir ni
migrar una base. Siempre devuelve un objeto JSON con exit code `0`.

```bash
dbox context
```

Proyecto encontrado:

```json
{
  "status": "found",
  "cwd": "/workspace/src",
  "project_directory": "/workspace",
  "dbox_directory": "/workspace/.dbox",
  "database": "/workspace/.dbox/data.db"
}
```

Limite de proyecto incompleto, sin buscar una base en un ancestro:

```json
{
  "status": "incomplete",
  "cwd": "/workspace/module",
  "project_directory": "/workspace/module",
  "dbox_directory": "/workspace/module/.dbox",
  "database": "/workspace/module/.dbox/data.db"
}
```

Sin una carpeta `.dbox`:

```json
{
  "status": "not_found",
  "cwd": "/tmp/workspace",
  "project_directory": null,
  "dbox_directory": null,
  "database": null
}
```

## Mantenimiento de la base de datos

Los comandos `backup` y `doctor` resuelven la base `.dbox/data.db` mas cercana
con las reglas normales de descubrimiento. No reciben payload JSON ni aplican
migraciones de EF Core.

### `dbox backup`

`backup` crea una copia online consistente mediante SQLite. La carpeta de
destino se crea bajo `.dbox/backups` y el nombre usa un timestamp UTC con el
formato `data-<YYYYMMDD>T<HHmmssfff>Z.db`. La ruta devuelta es relativa a la
raiz del proyecto resuelto, no al directorio desde el que se ejecuta el comando.

```bash
dbox backup
```

Respuesta exitosa:

```json
{
  "database": ".dbox/data.db",
  "backup": ".dbox/backups/data-20260821T120000000Z.db"
}
```

El origen se abre sin permiso de escritura y la copia no modifica la base
fuente. Si no se encuentra una base de proyecto, devuelve `database_not_found`
con exit code `4`. Si no se puede crear la carpeta o completar la copia,
devuelve `database_error` con exit code `4` y no informa un backup exitoso.

### `dbox doctor`

`doctor` es estrictamente read-only. No crea carpetas, no escribe archivos, no
repara la base y no aplica migraciones. Comprueba apertura, integridad SQLite,
migraciones conocidas pendientes y permisos inspeccionables.

```bash
dbox doctor
```

Respuesta para una base sana:

```json
{
  "database": ".dbox/data.db",
  "exists": true,
  "can_open": true,
  "integrity": "ok",
  "pending_migrations": [],
  "permissions": {
    "database_readable": true,
    "database_writable": true,
    "backup_directory_writable": null
  }
}
```

`integrity` puede ser `ok`, `failed` o `not_checked`. Si la base no se puede
abrir, `can_open` es `false`, `integrity` es `not_checked` y
`pending_migrations` es `null`. Un diagnostico no saludable que se puede
construir sigue siendo una respuesta exitosa de `doctor`; los fallos que
impiden construir el diagnostico completo devuelven `database_error`.

Los campos de `permissions` son informativos. Cada valor puede ser `true`,
`false` o `null` cuando la plataforma o la ausencia de la carpeta impide
determinarlo sin realizar una escritura. La carpeta `.dbox/backups` no se crea
como parte de `doctor`.

Si no se encuentra una base de proyecto, `doctor` devuelve el error
`database_not_found` con exit code `4` y no crea archivos del proyecto.

## Schema del catalogo

### `dbox activity schema`

El comando canonico es `activity schema`. No existe `activity --schema`.

```bash
dbox activity schema
```

Respuesta:

```json
{
  "entities": {
    "activity": {
      "fields": {
        "id": {
          "name": "id",
          "type": "integer",
          "required": false,
          "generated": true,
          "mutable": false,
          "description": "Identificador entero generado automaticamente."
        },
        "created_at": {
          "name": "created_at",
          "type": "datetime",
          "required": false,
          "generated": true,
          "mutable": false,
          "description": "Fecha y hora UTC generada al crear la actividad."
        },
        "type": {
          "name": "type",
          "type": "string",
          "required": true,
          "generated": false,
          "mutable": true,
          "description": "Clasificacion extensible de la actividad."
        },
        "title": {
          "name": "title",
          "type": "string",
          "required": true,
          "generated": false,
          "mutable": true,
          "maxLength": 200,
          "description": "Titulo breve de la actividad."
        },
        "description": {
          "name": "description",
          "type": "string",
          "required": true,
          "generated": false,
          "mutable": true,
          "description": "Descripcion de lo realizado."
        },
        "status": {
          "name": "status",
          "type": "string",
          "required": true,
          "generated": false,
          "mutable": true,
          "enum": [
            "pending",
            "in_progress",
            "completed"
          ],
          "description": "Estado actual de la actividad."
        },
        "source": {
          "name": "source",
          "type": "string",
          "required": true,
          "generated": false,
          "mutable": true,
          "description": "Origen o motivacion de la actividad."
        },
        "area": {
          "name": "area",
          "type": "string",
          "required": true,
          "generated": false,
          "mutable": true,
          "description": "Area funcional o tecnica afectada."
        },
        "result": {
          "name": "result",
          "type": "string",
          "required": true,
          "generated": false,
          "mutable": true,
          "description": "Resultado concreto obtenido."
        },
        "impact": {
          "name": "impact",
          "type": "string",
          "required": true,
          "generated": false,
          "mutable": true,
          "description": "Mejora, beneficio o consecuencia producida."
        },
        "effort": {
          "name": "effort",
          "type": "string",
          "required": true,
          "generated": false,
          "mutable": true,
          "enum": [
            "low",
            "medium",
            "high",
            "very-high"
          ],
          "description": "Estimacion cualitativa del esfuerzo."
        },
        "reference": {
          "name": "reference",
          "type": "string",
          "required": false,
          "generated": false,
          "mutable": true,
          "nullable": true,
          "description": "Referencia textual opcional relacionada."
        },
        "metadata": {
          "name": "metadata",
          "type": "json",
          "required": false,
          "generated": false,
          "mutable": true,
          "nullable": true,
          "description": "Objeto JSON opcional con informacion adicional extensible."
        }
      }
    }
  }
}
```

## Crear actividades

### `dbox activity add --json '<objeto>'` o `--json-file <path>`

La entrada llega completa en exactamente una fuente JSON. `--json-file -`
permite recibir el objeto desde `stdin`.

```bash
dbox activity add \
  --json '{"type":"implementation","title":"Implementar refresh token","description":"Agrega soporte de refresh token","status":"completed","source":"manual","area":"backend","result":"El flujo queda disponible","impact":"Mejora la continuidad de sesion","effort":"medium"}'
```

Respuesta:

```json
{
  "id": 15,
  "created_at": "2026-08-20T11:30:00Z",
  "type": "implementation",
  "title": "Implementar refresh token",
  "description": "Agrega soporte de refresh token",
  "status": "completed",
  "source": "manual",
  "area": "backend",
  "result": "El flujo queda disponible",
  "impact": "Mejora la continuidad de sesion",
  "effort": "medium",
  "reference": null,
  "metadata": null
}
```

La misma entrada puede leerse desde un archivo UTF-8 o desde `stdin`:

```bash
dbox activity add --json-file activity.json
cat activity.json | dbox activity add --json-file -
```

Reglas de entrada:

- `type`, `title`, `description`, `status`, `source`, `area`, `result`, `impact`
  y `effort` son obligatorios.
- `title` no puede estar vacio y admite hasta 200 caracteres.
- `status` debe ser `pending`, `in_progress` o `completed`.
- `effort` debe ser `low`, `medium`, `high` o `very-high`.
- `reference` y `metadata` son opcionales; `metadata` debe ser un objeto JSON o
  `null`.
- `id` y `created_at` no se aceptan porque son generados.
- Las propiedades desconocidas se rechazan.
- `--json` y `--json-file` no pueden usarse juntos.

## Listar y filtrar actividades

### `dbox activity list`

Sin `--json` ni `--json-file`, lista todas las actividades sin filtros. Esta
forma es equivalente a enviar `--json '{}'`.

```bash
dbox activity list
```

Respuesta:

```json
{
  "items": [
    {
      "id": 15,
      "created_at": "2026-08-20T11:30:00Z",
      "type": "implementation",
      "title": "Implementar refresh token",
      "description": "Agrega soporte de refresh token",
      "status": "completed",
      "source": "manual",
      "area": "backend",
      "result": "El flujo queda disponible",
      "impact": "Mejora la continuidad de sesion",
      "effort": "medium",
      "reference": null,
      "metadata": null
    }
  ],
  "pagination": {
    "skip": 0,
    "take": 100,
    "total": 1,
    "has_more": false
  }
}
```

Las actividades se ordenan siempre por `created_at ASC`. Cuando dos registros
tienen la misma fecha de creacion, se usa `id ASC` como desempate.

### `dbox activity list --json '<objeto>'` o `--json-file <path>` filtrado por tipo

```bash
dbox activity list --json '{"type":"research"}'
```

Respuesta:

```json
{
  "items": [
    {
      "id": 16,
      "created_at": "2026-08-20T11:31:00Z",
      "type": "research",
      "title": "Evaluar cache",
      "description": "Comparar opciones",
      "status": "pending",
      "source": "research",
      "area": "infrastructure",
      "result": "La alternativa queda evaluada",
      "impact": "Reduce incertidumbre",
      "effort": "low",
      "reference": null,
      "metadata": null
    }
  ],
  "pagination": {
    "skip": 0,
    "take": 100,
    "total": 1,
    "has_more": false
  }
}
```

### `dbox activity list --json '<objeto>'` filtrado por estado

```bash
dbox activity list --json '{"status":"completed"}'
```

Respuesta:

```json
{
  "items": [
    {
      "id": 15,
      "created_at": "2026-08-20T11:30:00Z",
      "type": "implementation",
      "title": "Implementar refresh token",
      "description": "Agrega soporte de refresh token",
      "status": "completed",
      "source": "manual",
      "area": "backend",
      "result": "El flujo queda disponible",
      "impact": "Mejora la continuidad de sesion",
      "effort": "medium",
      "reference": null,
      "metadata": null
    }
  ],
  "pagination": {
    "skip": 0,
    "take": 100,
    "total": 1,
    "has_more": false
  }
}
```

### `dbox activity list --json '<objeto>'` con filtros combinados

```bash
dbox activity list \
  --json '{"type":"research","status":"pending","area":"backend","source":"openspec","effort":"low","created_from":"2026-01-01T00:00:00Z","created_to":"2026-12-31T23:59:59Z","title":"refresh","description":"token"}'
```

Respuesta cuando no hay coincidencias:

```json
{
  "items": [],
  "pagination": {
    "skip": 0,
    "take": 100,
    "total": 0,
    "has_more": false
  }
}
```

### Paginacion con `--skip` y `--take`

`--skip` indica cuantos registros ordenados se omiten. `--take` indica el
maximo de registros que se devuelven. La respuesta siempre incluye los
metadatos de paginacion.

```bash
dbox activity list \
  --skip 10 \
  --take 10
```

Reglas de paginacion:

- `--skip` es opcional y su valor predeterminado es `0`.
- `--take` es opcional y su valor predeterminado es `100`.
- `--all` elimina el limite; en ese caso `pagination.take` es `null`.
- `--all` y `--take` no pueden usarse juntos.
- Ambos valores deben ser enteros mayores o iguales que `0`.
- El orden se aplica antes de `skip` y `take`.
- `pagination.total` cuenta todas las coincidencias antes de paginar.
- `pagination.has_more` indica si quedan coincidencias despues de la pagina.

Los filtros aceptados en `--json` y `--json-file` son `type`, `status`, `area`,
`source`, `effort`, `created_from`, `created_to`, `title` y `description`.
Los cinco primeros comparan el valor exacto. `created_from` y `created_to` son
fechas UTC ISO 8601 con sufijo `Z` y sus limites son inclusivos; el primero no
puede ser posterior al segundo. `title` y `description` realizan una busqueda
parcial insensible a mayusculas. Todos los filtros se combinan con `AND`.

## Contar actividades

### `dbox activity count`

Sin `--json` ni `--json-file`, devuelve la cantidad total de actividades. Esta
forma es equivalente a `dbox activity count --json '{}'`.

```bash
dbox activity count
```

Respuesta:

```json
{
  "count": 16
}
```

`count` acepta los mismos filtros opcionales que `list` mediante `--json` o
`--json-file`, sin paginacion, por lo que tambien puede devolver la cantidad de
registros de una busqueda concreta:

```bash
dbox activity count --json '{"type":"research","status":"pending"}'
```

Tambien puede recibir el filtro desde `stdin`:

```bash
cat filter.json | dbox activity count --json-file -
```

Respuesta:

```json
{
  "count": 1
}
```

### Extraer los ultimos 10 registros

Como `list` ordena ascendentemente por `created_at`, el offset para obtener los
ultimos diez registros se calcula con el total:

```bash
total=$(dbox activity count | jq -r '.count')
skip=$(( total > 10 ? total - 10 : 0 ))
dbox activity list --skip "$skip" --take 10
```

El resultado contiene los ultimos diez registros, pero conserva el orden
ascendente por `created_at`.

## Obtener una actividad

### `dbox activity get <id>`

El identificador siempre es un argumento posicional.

```bash
dbox activity get 15
```

Respuesta:

```json
{
  "id": 15,
  "created_at": "2026-08-20T11:30:00Z",
  "type": "implementation",
  "title": "Implementar refresh token",
  "description": "Agrega soporte de refresh token",
  "status": "completed",
  "source": "manual",
  "area": "backend",
  "result": "El flujo queda disponible",
  "impact": "Mejora la continuidad de sesion",
  "effort": "medium",
  "reference": null,
  "metadata": null
}
```

## Actualizar una actividad

### `dbox activity update <id> --json '<objeto>'` o `--json-file <path>`

El identificador es un argumento posicional. El objeto de la fuente JSON
contiene solamente los campos que se desean modificar. Se debe proporcionar
exactamente una fuente; `--json-file -` lee desde `stdin`.

```bash
dbox activity update 15 --json '{"status":"in_progress"}'
```

Respuesta:

```json
{
  "id": 15,
  "created_at": "2026-08-20T11:30:00Z",
  "type": "implementation",
  "title": "Implementar refresh token",
  "description": "Agrega soporte de refresh token",
  "status": "in_progress",
  "source": "manual",
  "area": "backend",
  "result": "El flujo queda disponible",
  "impact": "Mejora la continuidad de sesion",
  "effort": "medium",
  "reference": null,
  "metadata": null
}
```

Actualizacion parcial de varios campos:

```bash
dbox activity update 15 \
  --json '{"title":"Implementar refresh token con cookie","description":"Cookie HttpOnly"}'
```

Actualizar desde un archivo o limpiar un campo opcional:

```bash
dbox activity update 15 --json-file update.json
dbox activity update 15 --json '{"reference":null,"metadata":null}'
```

Respuesta:

```json
{
  "id": 15,
  "created_at": "2026-08-20T11:30:00Z",
  "type": "implementation",
  "title": "Implementar refresh token",
  "description": "Cookie HttpOnly",
  "status": "in_progress",
  "source": "manual",
  "area": "backend",
  "result": "El flujo queda disponible",
  "impact": "Mejora la continuidad de sesion",
  "effort": "medium",
  "reference": null,
  "metadata": null
}
```

Reglas de entrada:

- Se puede modificar `type`, `title`, `description`, `status`, `source`, `area`,
  `result`, `impact`, `effort`, `reference` y `metadata`.
- Debe existir al menos un campo modificable en el objeto JSON.
- Los campos obligatorios no aceptan `null`, texto vacio ni texto formado solo
  por espacios.
- `reference` y `metadata` pueden recibir `null` para limpiarse.
- `id` y `created_at` no se aceptan en el objeto JSON.
- Las propiedades desconocidas se rechazan.
- Los campos omitidos conservan su valor anterior.

## Eliminar una actividad

### Eliminacion persistente: `dbox activity delete <id> --yes`

La opcion `--yes` es obligatoria para persistir la eliminacion. No se muestra
una confirmacion interactiva ni se usa una papelera.

```bash
dbox activity delete 15 --yes
```

Respuesta:

```json
{
  "id": 15,
  "deleted": true
}
```

### Previsualizacion: `dbox activity delete <id> --dry-run`

`--dry-run` resuelve y lee la actividad sin aplicar migraciones ni guardar
cambios. No requiere `--yes`.

```bash
dbox activity delete 15 --dry-run
```

Respuesta:

```json
{
  "id": 15,
  "deleted": false,
  "dry_run": true,
  "activity": {
    "id": 15,
    "created_at": "2026-08-20T11:30:00Z",
    "type": "implementation",
    "title": "Implementar refresh token",
    "description": "Agrega soporte de refresh token",
    "status": "completed",
    "source": "manual",
    "area": "backend",
    "result": "El flujo queda disponible",
    "impact": "Mejora la continuidad de sesion",
    "effort": "medium",
    "reference": null,
    "metadata": null
  }
}
```

Cuando se proporcionan ambas opciones, la previsualizacion tiene prioridad y
no se elimina la actividad:

```bash
dbox activity delete 15 --yes --dry-run
```

Sin `--yes` ni `--dry-run`, el comando devuelve `validation_error` y no cambia
la base.

## Errores

Los errores se escriben exclusivamente en `stderr`. En todos los casos,
`stdout` queda vacio.

### Fuentes JSON incompatibles

Cuando se proporcionan `--json` y `--json-file` al mismo tiempo, la CLI no lee
ninguna fuente ni ejecuta la operacion.

```bash
dbox activity count --json '{}' --json-file filters.json
```

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Specify either '--json' or '--json-file', not both.",
    "details": [
      {
        "field": "json",
        "message": "Specify either '--json' or '--json-file', not both."
      }
    ]
  }
}
```

Exit code: `2`.

### Archivo JSON ilegible

Un archivo inexistente, inaccesible o que no pueda leerse como UTF-8 devuelve
un error de validacion determinista.

```json
{
  "error": {
    "code": "validation_error",
    "message": "Unable to read JSON input.",
    "details": [
      {
        "field": "json",
        "message": "Unable to read JSON input."
      }
    ]
  }
}
```

Exit code: `2`.

### Entrada JSON invalida

```bash
dbox activity add --json '{"type":'
```

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "validation_error",
    "message": "JSON input must be a valid JSON object.",
    "details": [
      {
        "field": "json",
        "message": "JSON input must be a valid JSON object."
      }
    ]
  }
}
```

Exit code: `2`.

### Opciones de consulta invalidas

`--all` no puede combinarse con un `--take` explicito:

```bash
dbox activity list --all --take 100
```

La respuesta es:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Options '--all' and '--take' cannot be used together.",
    "details": [
      {
        "field": "take",
        "message": "Options '--all' and '--take' cannot be used together."
      }
    ]
  }
}
```

Los valores negativos de `--skip` o `--take`, las propiedades de filtro
desconocidas, los enums invalidos, las fechas que no terminen en `Z` y los
rangos UTC invertidos tambien devuelven `validation_error` con exit code `2`.

### Campo requerido ausente

Entrada:

```json
{
  "title": "Sin tipo"
}
```

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Invalid activity.",
    "details": [
      {
        "field": "type",
        "message": "Field must be a non-blank value."
      }
    ]
  }
}
```

Exit code: `2`.

### Valor de enum invalido

Entrada:

```json
{
  "type": "research",
  "title": "Mayusculas no permitidas",
  "description": "Ejemplo",
  "status": "completed",
  "source": "manual",
  "area": "backend",
  "result": "Resultado",
  "impact": "Impacto",
  "effort": "Medium"
}
```

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Invalid activity.",
    "details": [
      {
        "field": "effort",
        "message": "Value must be one of: low, medium, high, very-high."
      }
    ]
  }
}
```

Exit code: `2`.

### Actividad inexistente

Comando:

```bash
dbox activity get 999
```

Respuesta en `stderr` para `get`, `update` o `delete`:

```json
{
  "error": {
    "code": "resource_not_found",
    "message": "Activity 999 not found."
  }
}
```

Exit code: `3`.

### Base no encontrada

Comando:

```bash
dbox activity list --json '{}'
```

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "database_not_found",
    "message": "No dbox database found. Run 'dbox init' to initialize this directory."
  }
}
```

Exit code: `4`.

### Error de base de datos

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "database_error",
    "message": "Database error."
  }
}
```

Exit code: `4`.

### Comando desconocido o sintaxis invalida

Comando:

```bash
dbox activity unknown
```

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Invalid command syntax.",
    "details": [
      {
        "field": "command",
        "message": "Unknown command."
      }
    ]
  }
}
```

Exit code: `2`.

## Comandos sin argumento

### `dbox`

```bash
dbox
```

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Invalid command syntax.",
    "details": [
      {
        "field": "command",
        "message": "A command is required."
      }
    ]
  }
}
```

Exit code: `2`.

### `dbox activity`

```bash
dbox activity
```

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Invalid command syntax.",
    "details": [
      {
        "field": "command",
        "message": "A catalog command is required."
      }
    ]
  }
}
```

Exit code: `2`.

## Formas eliminadas

Las siguientes formas ya no serian validas:

| Forma eliminada | Forma nueva |
| --- | --- |
| `dbox activity --schema` | `dbox activity schema` |
| `dbox activity schema --json` | `dbox activity schema` |
| `dbox activity list --output json` | `dbox activity list --json '{}'` |
| `dbox activity list --type research` | `dbox activity list --json '{"type":"research"}'` |
| `printf '%s' '{...}' \| dbox activity add` | `printf '%s' '{...}' \| dbox activity add --json-file -` |
| `dbox activity add --type research --title Test` | `dbox activity add --json '{"type":"research","title":"Test"}'` |
| `printf '%s' '{"status":"completed"}' \| dbox activity update 15` | `printf '%s' '{"status":"completed"}' \| dbox activity update 15 --json-file -` |
| `dbox activity update 15 --status completed` | `dbox activity update 15 --json '{"status":"completed"}'` |
| `dbox --schema` | `dbox activity schema` |
| `dbox activity delete 15` | `dbox activity delete 15 --yes` o `dbox activity delete 15 --dry-run` |

Todas las formas eliminadas devuelven `validation_error` en JSON y no ejecutan
ninguna operacion sobre la base.

## Codigos de salida

| Codigo | Significado |
| --- | --- |
| `0` | Operacion exitosa |
| `1` | Error inesperado |
| `2` | Error de validacion o sintaxis |
| `3` | Recurso no encontrado |
| `4` | Base no encontrada o error de base de datos |

## Decisiones incorporadas

- Los objetos JSON de actividad y los filtros rechazan propiedades desconocidas
  y valores que no tengan la forma documentada.
- Los errores de fuentes JSON, filtros y paginacion conservan mensajes
  deterministas dentro del envelope `validation_error`.
- `count` acepta exactamente los mismos filtros que `list` y no pagina sus
  resultados.
- `list` calcula `total` sobre la consulta filtrada antes de aplicar
  `skip`/`take`; la respuesta incluye `items` y `pagination`.
- La CLI conserva los comandos textuales (`activity add`, `activity list`, etc.)
  como selectores publicos.
