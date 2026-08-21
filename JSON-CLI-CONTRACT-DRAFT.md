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
- `--json '<objeto>'` es la unica opcion para enviar un payload JSON.
- `--json` es obligatorio en `add` y `update`.
- `--json` es opcional en `list` y `count`; si se omite, no hay filtros.
- No existen `--output text` ni `--output json`.
- No existen aliases de comandos.
- `add`, `list`, `count` y `update` reciben su payload mediante `--json`.
- `get`, `update` y `delete` reciben el identificador como argumento
  posicional: `get 1`, `update 1` y `delete 1`.
- `init`, `context`, `backup`, `doctor` y `activity schema` no reciben payload.
- `activity delete <id> --yes` es la forma persistente y requiere confirmacion
  explicita.
- `activity delete <id> --dry-run` previsualiza sin migrar ni persistir; si se
  combinan `--dry-run` y `--yes`, prevalece `--dry-run`.
- La CLI no lee payloads desde `stdin`.
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
| `dbox activity add --json '{...}'` | Objeto de actividad | Actividad creada |
| `dbox activity list [--json '{...}']` | Filtros opcionales | Arreglo de actividades |
| `dbox activity count [--json '{...}']` | Filtros opcionales | Objeto con `count` |
| `dbox activity get 15` | ID posicional | Actividad solicitada |
| `dbox activity update 15 --json '{...}'` | Campos modificables | Actividad actualizada |
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
  dbox activity add --json <json> [options]

Options:
  --json <json>         Activity input as a JSON object.
  --help                Show command line help.
```

`list --help` tambien muestra `--skip` y `--take`. `get`, `update` y `delete`
muestran que el ID es un argumento posicional. `delete --help` tambien muestra
`--yes` y `--dry-run`. Los detalles de campos y enums se descubren con `dbox activity schema`.

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
  --json <json>         Optional filters as a JSON object.
  --skip <skip>         Number of ordered records to skip. (Default: 0)
  --take <take>         Maximum number of records to return.
  --help                Show command line help.
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
          "type": "integer",
          "generated": true,
          "mutable": false
        },
        "created_at": {
          "type": "datetime",
          "generated": true,
          "mutable": false
        },
        "type": {
          "type": "string",
          "required": true,
          "enum": [
            "research",
            "implementation",
            "bugfix",
            "maintenance"
          ]
        },
        "title": {
          "type": "string",
          "required": true,
          "maxLength": 200
        },
        "description": {
          "type": "string",
          "required": false
        },
        "status": {
          "type": "string",
          "required": true,
          "default": "completed",
          "enum": [
            "pending",
            "in_progress",
            "completed"
          ]
        }
      }
    }
  }
}
```

## Crear actividades

### `dbox activity add --json '<objeto>'`

La entrada llega completa en la opcion `--json`.

```bash
dbox activity add \
  --json '{"type":"implementation","title":"Implementar refresh token"}'
```

Respuesta:

```json
{
  "id": 15,
  "created_at": "2026-08-20T11:30:00Z",
  "type": "implementation",
  "title": "Implementar refresh token",
  "description": null,
  "status": "completed"
}
```

Con todos los campos modificables:

```bash
dbox activity add \
  --json '{"type":"research","title":"Evaluar cache","description":"Comparar opciones","status":"pending"}'
```

Respuesta:

```json
{
  "id": 16,
  "created_at": "2026-08-20T11:31:00Z",
  "type": "research",
  "title": "Evaluar cache",
  "description": "Comparar opciones",
  "status": "pending"
}
```

Reglas de entrada:

- `type` es obligatorio.
- `title` es obligatorio, no puede estar vacio y admite hasta 200 caracteres.
- `description` es opcional y puede ser `null`.
- `status` es opcional; su valor predeterminado es `completed`.
- `id` y `created_at` no se aceptan porque son generados.
- Las propiedades desconocidas se rechazan.

## Listar y filtrar actividades

### `dbox activity list`

Sin `--json`, lista todas las actividades sin filtros. Esta forma es equivalente
a enviar `--json '{}'`.

```bash
dbox activity list
```

Respuesta:

```json
[
  {
    "id": 15,
    "created_at": "2026-08-20T11:30:00Z",
    "type": "implementation",
    "title": "Implementar refresh token",
    "description": null,
    "status": "completed"
  },
  {
    "id": 16,
    "created_at": "2026-08-20T11:31:00Z",
    "type": "research",
    "title": "Evaluar cache",
    "description": "Comparar opciones",
    "status": "pending"
  }
]
```

Las actividades se ordenan siempre por `created_at ASC`. Cuando dos registros
tienen la misma fecha de creacion, se usa `id ASC` como desempate.

### `dbox activity list --json '<objeto>'` filtrado por tipo

```bash
dbox activity list --json '{"type":"research"}'
```

Respuesta:

```json
[
  {
    "id": 16,
    "created_at": "2026-08-20T11:31:00Z",
    "type": "research",
    "title": "Evaluar cache",
    "description": "Comparar opciones",
    "status": "pending"
  }
]
```

### `dbox activity list --json '<objeto>'` filtrado por estado

```bash
dbox activity list --json '{"status":"completed"}'
```

Respuesta:

```json
[
  {
    "id": 15,
    "created_at": "2026-08-20T11:30:00Z",
    "type": "implementation",
    "title": "Implementar refresh token",
    "description": null,
    "status": "completed"
  }
]
```

### `dbox activity list --json '<objeto>'` con filtros combinados

```bash
dbox activity list \
  --json '{"type":"research","status":"pending"}'
```

Respuesta cuando no hay coincidencias:

```json
[]
```

### Paginacion con `--skip` y `--take`

`--skip` indica cuantos registros ordenados se omiten. `--take` indica el
maximo de registros que se devuelven.

```bash
dbox activity list \
  --skip 10 \
  --take 10
```

Reglas de paginacion:

- `--skip` es opcional y su valor predeterminado es `0`.
- `--take` es opcional; si se omite, devuelve todos los registros restantes.
- Ambos valores deben ser enteros mayores o iguales que `0`.
- El orden se aplica antes de `skip` y `take`.

Los filtros aceptados en `--json` son `type` y `status`. Los valores deben
coincidir exactamente con los enums del schema.

## Contar actividades

### `dbox activity count`

Sin `--json`, devuelve la cantidad total de actividades. Esta forma es
equivalente a `dbox activity count --json '{}'`.

```bash
dbox activity count
```

Respuesta:

```json
{
  "count": 16
}
```

`count` acepta los mismos filtros opcionales que `list`, por lo que tambien
puede devolver la cantidad de registros de una busqueda concreta:

```bash
dbox activity count --json '{"type":"research","status":"pending"}'
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
  "description": null,
  "status": "completed"
}
```

## Actualizar una actividad

### `dbox activity update <id> --json '<objeto>'`

El identificador es un argumento posicional. El objeto de `--json` contiene
solamente los campos que se desean modificar.

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
  "description": null,
  "status": "in_progress"
}
```

Actualizacion parcial de varios campos:

```bash
dbox activity update 15 \
  --json '{"title":"Implementar refresh token con cookie","description":"Cookie HttpOnly"}'
```

Limpiar la descripcion:

```bash
dbox activity update 15 --json '{"description":null}'
```

Respuesta:

```json
{
  "id": 15,
  "created_at": "2026-08-20T11:30:00Z",
  "type": "implementation",
  "title": "Implementar refresh token",
  "description": null,
  "status": "in_progress"
}
```

Reglas de entrada:

- Se puede modificar `type`, `title`, `description` y `status`.
- Debe existir al menos un campo modificable en el objeto JSON.
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

### Entrada JSON invalida

```bash
dbox activity add --json '{"type":'
```

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Invalid request.",
    "details": [
      {
        "field": "input",
        "message": "The JSON input is invalid."
      }
    ]
  }
}
```

Exit code: `2`.

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
    "message": "Invalid request.",
    "details": [
      {
        "field": "type",
        "message": "Field is required."
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
  "type": "Research",
  "title": "Mayusculas no permitidas"
}
```

Respuesta en `stderr`:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Invalid request.",
    "details": [
      {
        "field": "type",
        "message": "Value must be one of: research, implementation, bugfix, maintenance."
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
| `printf '%s' '{...}' \| dbox activity add` | `dbox activity add --json '{...}'` |
| `dbox activity add --type research --title Test` | `dbox activity add --json '{"type":"research","title":"Test"}'` |
| `printf '%s' '{"status":"completed"}' \| dbox activity update 15` | `dbox activity update 15 --json '{"status":"completed"}'` |
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

## Decisiones pendientes de revision

- Confirmar si todos los objetos deben rechazar propiedades desconocidas,
  incluido el objeto de filtros de `list`.
- Confirmar si el mensaje de error debe ser siempre `Invalid request.` o si
  debe conservar mensajes mas especificos por comando.
- Confirmar si `count` debe aceptar los mismos filtros que `list` o contar
  siempre todos los registros sin importar el payload.
- Definir si el calculo de `count` y la pagina posterior de `list` necesitan
  una garantia de consistencia cuando se crean o eliminan registros entre
  ambas invocaciones.
- Confirmar si los comandos textuales (`activity add`, `activity list`, etc.)
  son aceptables o si se quiere una unica entrada JSON con un campo
  `command`.
