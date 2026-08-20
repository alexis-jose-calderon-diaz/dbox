# dbox: Definicion del Proyecto

## Estado de este repositorio

Este documento es la definicion funcional y tecnica de `dbox`. En la entrega
inicial del repositorio solo existe este archivo. No incluye codigo fuente,
proyectos .NET, configuracion de compilacion, migraciones, pruebas ni
artefactos de base de datos.

La implementacion posterior debe ajustarse a este documento. Cuando una
decision aqui definida cambie, primero se debe actualizar este contrato y luego
la implementacion, las pruebas y la documentacion de uso.

## Proposito

`dbox` es una CLI local para administrar catalogos de datos estructurados en
SQLite sin que las personas, scripts o agentes de IA tengan que escribir SQL.
La CLI es el contrato publico de acceso a los datos.

La primera version administra el catalogo `activity`, que contiene una sola
entidad fija. La herramienta esta preparada para incorporar catalogos
adicionales, como `command` y `skill`, pero no permitira que cada usuario defina
entidades, columnas o esquemas dinamicos.

El producto prioriza:

- simplicidad de uso desde terminal;
- comportamiento determinista;
- un contrato JSON estable para automatizacion;
- una base de datos local e independiente por proyecto;
- validacion previa a cualquier cambio persistente;
- migraciones versionadas y mantenibles;
- pocas dependencias y una arquitectura pequena.

## Nombre y terminologia

- El nombre del repositorio, ejecutable y producto es `dbox`.
- Una **base de proyecto** es el archivo SQLite situado en `.dbox/data.db`.
- Un **directorio de proyecto dbox** es el directorio que contiene la carpeta
  `.dbox` mas cercana al directorio de trabajo actual.
- Una **base inicializada** es una base que existe, contiene el historial de
  migraciones de EF Core y tiene todas las migraciones conocidas aplicadas.
- Una **operacion de infraestructura compartida** afecta a todos los catalogos
  del proyecto y vive en la raiz de la CLI.
- Un **catalogo** es un grupo de comandos que administra sus propios datos y
  contrato.
- Un **comando de datos** es `add`, `list`, `count`, `get`, `update` o `delete`
  dentro de un catalogo.

No existe una base global, una carpeta de usuario, un servidor remoto ni un
almacenamiento compartido por defecto.

### Jerarquia de comandos

La raiz de `dbox` contiene exclusivamente operaciones de infraestructura
compartida y los grupos de catalogos disponibles. `init` es la unica operacion
de infraestructura del MVP: crea la base local del proyecto y aplica todas las
migraciones conocidas por la version instalada, incluidas las tablas de
cualquier catalogo futuro.

Las operaciones especificas de datos y contrato viven dentro de su catalogo:

```text
dbox
├── init
└── activity
    ├── schema
    ├── add
    ├── list
    ├── count
    ├── get
    ├── update
    └── delete
```

No existen `activity init`, `command init` ni `skill init`. Los catalogos
futuros se agregaran como grupos hermanos de `activity` sin volver a exponer
sus operaciones directamente en la raiz.

## Alcance del MVP

El MVP debe permitir:

1. Inicializar una base local por proyecto.
2. Descubrir automaticamente la base de proyecto mas cercana.
3. Mostrar el contrato de datos y sus reglas para humanos y agentes.
4. Crear, listar, contar, obtener, actualizar y eliminar actividades.
5. Recibir entradas estructuradas mediante JSON donde corresponda.
6. Producir salidas JSON estables.
7. Aplicar automaticamente migraciones internas pendientes.
8. Informar validaciones, recursos inexistentes y errores de base con codigos
   de salida consistentes.

## Fuera de alcance del MVP

No se implementaran en el MVP:

- entidades, columnas o schemas definidos por usuarios;
- consultas SQL arbitrarias;
- una opcion `--database` para anular la base de proyecto;
- MCP, HTTP, API REST, interfaz grafica o servicios en segundo plano;
- autenticacion, autorizacion, usuarios o sincronizacion remota;
- importacion o exportacion de CSV, Excel o JSONL;
- reportes, estadisticas, dashboards o filtros avanzados;
- plugins, scripts de usuario o hooks;
- configuracion global compartida;
- Entity Framework Core como una capa generica de repositorios;
- abstracciones enterprise como mediator, AutoMapper o una jerarquia de capas
  Domain/Application/Infrastructure/Presentation.

## Modelo local por proyecto

### Inicializacion

El comando:

```bash
dbox init
```

siempre opera sobre el directorio actual. No busca una base en directorios
padre y no reutiliza una base global.

Si se ejecuta en `/proyecto`, la ubicacion resultante es:

```text
/proyecto/.dbox/data.db
```

Si se ejecuta en `/proyecto/src`, la ubicacion resultante es:

```text
/proyecto/src/.dbox/data.db
```

aunque `/proyecto/.dbox/data.db` ya exista. Esto permite crear un proyecto
dbox anidado de forma deliberada.

`init` debe:

1. Crear la carpeta `.dbox` del directorio actual si no existe.
2. Crear `.dbox/data.db` si no existe.
3. Ejecutar las migraciones pendientes de EF Core sobre esa base.
4. No eliminar, truncar, reemplazar ni sobrescribir una base existente.
5. Poder ejecutarse repetidamente sin alterar datos ya persistidos.

Su respuesta es siempre un objeto JSON con `database: ".dbox/data.db"` y uno
de los estados `initialized`, `already_initialized` o `migrated`. El primero se
usa para una base inexistente, el segundo para una base actualizada y el tercero
para una base existente con migraciones pendientes.

### Descubrimiento de la base

Todo comando excepto `init` debe resolver una base antes de hacer su trabajo:

```bash
dbox activity schema
dbox activity add --json '{"type":"research","title":"Evaluacion"}'
dbox activity list
dbox activity count
dbox activity get 1
dbox activity update 1 --json '{"status":"completed"}'
dbox activity delete 1
```

El algoritmo de descubrimiento debe estar centralizado en un unico componente,
por ejemplo `DboxLocator`, y ser el unico mecanismo usado por los comandos.

El algoritmo es el siguiente:

1. Normalizar el directorio de trabajo actual a una ruta absoluta.
2. Revisar si contiene una carpeta `.dbox`.
3. Si la carpeta no existe, repetir la revision en el directorio padre.
4. Continuar hasta llegar a la raiz del sistema de archivos.
5. Usar la primera carpeta `.dbox` encontrada; representa el proyecto mas
   cercano.
6. Comprobar que esa carpeta contiene `data.db`.
7. Si existe, devolver esa ruta como la unica base que usara el comando.

Ejemplo:

```text
/proyecto/.dbox/data.db
/proyecto/src/Features/Area
```

Al ejecutar `dbox activity list` desde `/proyecto/src/Features/Area`, se debe usar
`/proyecto/.dbox/data.db`.

Si un padre y un hijo contienen `.dbox`, prevalece siempre el mas cercano. Por
ejemplo, desde `/proyecto/modulo/src`, se usara
`/proyecto/modulo/.dbox/data.db` antes que `/proyecto/.dbox/data.db`.

Una carpeta `.dbox` sin `data.db` representa una inicializacion incompleta o
una base eliminada manualmente. Es un limite de proyecto: el descubrimiento se
detiene y no debe continuar hacia un `.dbox` situado mas arriba.

Si no se encuentra una base valida, el comando debe escribir en `stderr`:

```text
No dbox database found.
Run 'dbox init' to initialize this directory.
```

La ausencia de una base de proyecto retorna el exit code `4`.

### Independencia entre proyectos

Cada directorio inicializado contiene sus propios datos y su propio historial
de migraciones. Por ejemplo:

```text
/proyecto-a/.dbox/data.db
/proyecto-b/.dbox/data.db
```

Las actividades creadas en `proyecto-a` no son visibles ni modificables desde
`proyecto-b`. La CLI no debe buscar, crear ni consultar una base fuera del
directorio de proyecto resuelto.

## Stack previsto

La implementacion usara:

- .NET 10 y C#;
- `System.CommandLine` para el analisis de argumentos y ayuda de la CLI;
- `Microsoft.EntityFrameworkCore.Sqlite` para SQLite y migraciones;
- EF Core migrations generadas mediante `dotnet ef`;
- `xUnit` para las pruebas automatizadas.

La aplicacion no tendra una referencia directa a `Microsoft.Data.Sqlite` ni
usara `SqliteConnection`, `SqliteCommand` o SQL CRUD escrito a mano. El
proveedor `Microsoft.EntityFrameworkCore.Sqlite` puede depender
transitivamente de `Microsoft.Data.Sqlite`; esa dependencia es un detalle
interno del proveedor y no forma parte de la API ni del codigo de `dbox`.

No se utilizara Entity Framework Core para introducir repositorios genericos,
unit of work propios, DI compleja ni capas adicionales. Se usara un
`DboxDbContext` pequeno y una configuracion explicita de la entidad.

## Esquema fijo y migraciones

### Regla de evolucion

El esquema de `dbox` es fijo y pertenece a la version instalada de la
herramienta. Una version futura puede agregar tablas, columnas, indices o
transformaciones de datos, pero esos cambios se implementan como migraciones
internas versionadas de EF Core.

Los usuarios no crean ni proporcionan migraciones. Las migraciones son parte
del codigo fuente versionado de `dbox`.

### Aplicacion de migraciones

`init` y todos los comandos de catalogo que resuelven una base de proyecto
deben ejecutar `Database.MigrateAsync()` antes de su operacion principal. Esto
incluye `activity schema`, para que una invocacion sobre una base existente
siempre la deje en la version conocida por la CLI.

Las migraciones se aplican de forma silenciosa en `activity add`,
`activity list`, `activity get`, `activity update`, `activity delete` y
`activity schema`: la salida normal de esos comandos no incluira mensajes
adicionales que rompan scripts o JSON. `init` es el unico comando que informa
explicitamente si inicializo o migro la base.

EF Core mantiene el estado aplicado en `__EFMigrationsHistory`. En SQLite, EF
Core tambien usa su mecanismo de bloqueo de migraciones para evitar que dos
procesos apliquen simultaneamente el mismo cambio.

Si una migracion falla, el comando debe terminar con exit code `4`, emitir un
error de base y no ejecutar despues la operacion solicitada. `dbox` no debe
intentar recuperar mediante SQL manual ni eliminar la base.

Las migraciones se generaran con una herramienta local versionada,
`dotnet-ef`, y se almacenaran junto al contexto, por ejemplo en
`src/Dbox/Database/Migrations/`. No se editaran manualmente migraciones ya
generadas y aplicadas; un cambio posterior se representa con una nueva
migracion.

## Entidad fija: activity

El MVP administra exclusivamente la entidad singular `activity`, persistida
en la tabla plural `activities`.

La forma SQL equivalente esperada es:

```sql
CREATE TABLE activities (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    created_at TEXT NOT NULL,
    type TEXT NOT NULL,
    title TEXT NOT NULL,
    description TEXT NULL,
    status TEXT NOT NULL
);
```

EF Core sera responsable de generar la migracion real. La configuracion del
modelo debe preservar estos nombres de tabla y columnas.

### Campos y reglas

| Campo | Tipo publico | Requerido | Generado | Modificable | Regla |
| --- | --- | --- | --- | --- | --- |
| `id` | integer | si | si | no | Entero autogenerado por SQLite. |
| `created_at` | datetime | si | si | no | Generado al crear en UTC, con formato ISO 8601 `YYYY-MM-DDTHH:mm:ssZ`. |
| `type` | string | si | no | si | Uno de `research`, `implementation`, `bugfix`, `maintenance`. |
| `title` | string | si | no | si | Texto no vacio de hasta 200 caracteres. |
| `description` | string | no | no | si | Texto opcional; puede ser `null`. |
| `status` | string | si | no | si | Uno de `pending`, `in_progress`, `completed`; el valor por defecto es `completed`. |

Los valores de `type` y `status` son sensibles a mayusculas y deben usarse
exactamente como se declaran. Un titulo formado solo por espacios se considera
vacio. La fecha de creacion no puede ser recibida ni modificada por la CLI.

### Fuente unica de reglas

Una definicion compartida, por ejemplo `ActivitySchema`, sera la fuente unica
de verdad de:

- los campos publicos y sus metadatos;
- campos generados y mutables;
- valores permitidos de `type` y `status`;
- valor por defecto de `status`;
- longitud maxima de `title`;
- validacion de entradas;
- restricciones equivalentes de la configuracion EF Core;
- salida humana y JSON del comando `activity schema`.

No se deben repetir manualmente valores de enum, limites o reglas en el
validador, el contexto EF Core y el comando `activity schema`.

## Contrato de la CLI

La ayuda textual de `dbox --help` y `dbox activity --help`, junto con el
contrato JSON de `dbox activity schema`, deben bastar para descubrir y usar la
herramienta. No existe `--database`, `--output`, ni un alias `--schema`.

Toda operacion exitosa escribe exactamente un valor JSON en `stdout`. Todo error
operacional o de sintaxis escribe exactamente un objeto JSON en `stderr`, sin
texto adicional en `stdout`. `--help` es la unica salida humana y no consulta
la base de datos.

### init y activity schema

```bash
dbox init
dbox activity schema
```

`init` inicializa exclusivamente el directorio actual y devuelve un objeto con
`database: ".dbox/data.db"` y estado `initialized`, `already_initialized` o
`migrated`. `activity schema` resuelve la base, aplica migraciones y devuelve el
contrato JSON estable bajo `entities.activity.fields`; no expone detalles de
SQLite o EF Core.

### activity add y update

```bash
dbox activity add --json '{"type":"implementation","title":"Implementa refresh token"}'
dbox activity update 15 --json '{"status":"completed","title":"Nuevo titulo"}'
```

`--json` es obligatorio en `add` y `update`. Los objetos de entrada aceptan
solo campos modificables; `type` y `title` son obligatorios al crear, `status`
usa `completed` por defecto, y `description: null` limpia la descripcion al
actualizar. Las opciones de campos individuales, propiedades desconocidas,
`id` y `created_at` se rechazan.

### activity list y count

```bash
dbox activity list
dbox activity list --json '{"type":"research","status":"completed"}'
dbox activity list --skip 10 --take 10
dbox activity count
dbox activity count --json '{"type":"research"}'
```

`--json` es opcional en `list` y `count`; cuando se proporciona, acepta
unicamente filtros `type` y `status`. `list` devuelve un arreglo JSON ordenado
por `created_at ASC` y luego `id ASC`; aplica `--skip` y `--take`, ambos
enteros no negativos, despues del ordenamiento. `count` devuelve un objeto
`{ "count": <integer> }` y aplica los mismos filtros sin paginacion.

### activity get y delete

```bash
dbox activity get 15
dbox activity delete 15
```

Los IDs permanecen como argumentos posicionales. `get` devuelve la actividad
completa y `delete` devuelve `{ "id": 15, "deleted": true }`.

## Validacion y errores

Toda la validacion de entradas ocurre antes de ejecutar `SaveChangesAsync()`.
Los errores de validacion son resultados esperados del contrato, no
excepciones usadas como flujo normal. Los errores se escriben exclusivamente en
`stderr` como un objeto JSON valido:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Invalid activity.",
    "details": [
      {
        "field": "type",
        "message": "Value must be one of: research, implementation, bugfix, maintenance."
      }
    ]
  }
}
```

Los codigos de error JSON son estables:

- `validation_error` para entradas invalidas o sintaxis de comando invalida;
- `resource_not_found` para una actividad inexistente;
- `database_not_found` para la ausencia de una base de proyecto;
- `database_error` para migraciones, apertura o persistencia fallida;
- `unexpected_error` para fallos no previstos.

Los exit codes minimos son:

| Codigo | Significado |
| --- | --- |
| `0` | Operacion exitosa. |
| `1` | Error inesperado. |
| `2` | Error de validacion o sintaxis de entrada. |
| `3` | Recurso solicitado no encontrado. |
| `4` | Base no encontrada, migracion fallida o error de base de datos. |

Las salidas exitosas se escriben en `stdout` y no incluyen encabezados,
mensajes auxiliares ni texto de diagnostico.

## Arquitectura futura minima

La implementacion debe reflejar la jerarquia publica de comandos sin agregar
capas generales, un contenedor de DI complejo ni repositorios genericos:

```text
src/
└── Dbox/
    ├── Program.cs
    ├── Cli/
    │   ├── DboxCli.cs
    │   ├── CliError.cs
    │   ├── CommandExecutor.cs
    │   └── ExitCodes.cs
    ├── Commands/
    │   ├── Root/
    │   │   └── RootCommand.cs
    │   ├── Init/
    │   │   └── InitCommand.cs
    │   └── Activity/
    │       ├── ActivityCommand.cs
    │       ├── Schema/
    │       │   └── SchemaCommand.cs
    │       ├── Add/
    │       │   └── AddCommand.cs
    │       ├── List/
    │       │   └── ListCommand.cs
    │       ├── Count/
    │       │   └── CountCommand.cs
    │       ├── Get/
    │       │   └── GetCommand.cs
    │       ├── Update/
    │       │   └── UpdateCommand.cs
    │       └── Delete/
    │           └── DeleteCommand.cs
    ├── Activities/
    │   ├── Activity.cs
    │   ├── ActivityView.cs
    │   ├── ActivitySchema.cs
    │   ├── ActivityInputParser.cs
    │   ├── ActivityValidation.cs
    │   └── ActivityRepository.cs
    ├── Database/
    │   ├── DboxLocation.cs
    │   ├── DboxLocator.cs
    │   ├── DboxDbContext.cs
    │   ├── DboxDbContextFactory.cs
    │   ├── DboxDatabase.cs
    │   └── Migrations/
    └── Output/
        ├── OutputWriter.cs
        ├── InitResponse.cs
        └── DeleteResponse.cs

tests/
└── Dbox.Tests/
    ├── Commands/
    │   ├── Root/
    │   ├── Init/
    │   └── Activity/
    │       ├── Schema/
    │       ├── Add/
    │       ├── List/
    │       ├── Get/
    │       ├── Update/
    │       └── Delete/
    ├── Integration/
    ├── Database/
    └── Support/
```

`Program.cs` y `DboxCli` componen los componentes de forma explicita. Cada
carpeta bajo `Commands` corresponde a una parte de la jerarquia publica de la
CLI y cada comando hoja define sus propias opciones, argumentos y accion.
`CommandExecutor` contiene unicamente la ejecucion transversal de salida,
errores y exit codes.

`Activities` contiene el soporte compartido del catalogo `activity`; por eso
`ActivityRepository` vive junto a la entidad, reglas y parser, y no bajo la
infraestructura global. `DboxLocator` centraliza la ubicacion; el contexto
recibe la ruta ya resuelta. `Dbox.Activities.Activity` conserva su namespace
porque forma parte de la identidad CLR usada por el model snapshot de EF Core.

Las pruebas mantienen un unico proyecto `Dbox.Tests`, con carpetas espejo para
comandos y carpetas separadas para integracion, base de datos y utilidades. Los
tests de cada caso siguen usando directorios temporales y bases independientes.

## Pruebas requeridas

Las pruebas deben usar directorios temporales y archivos SQLite temporales.
Nunca deben usar una ruta real del usuario ni compartir una base entre casos.

Como minimo se deben cubrir:

- `init` crea `.dbox/data.db` y aplica la migracion inicial;
- `init` repetido preserva datos existentes;
- dos directorios de proyecto usan bases distintas;
- un comando en un subdirectorio encuentra la base del padre;
- una base `.dbox` mas cercana prevalece sobre una base de un ancestro;
- `init` en un subdirectorio crea una base anidada y no modifica al padre;
- ausencia de `.dbox/data.db` devuelve el mensaje y exit code definidos;
- una `.dbox` incompleta bloquea la busqueda hacia un ancestro;
- las migraciones pendientes se aplican antes de cada comando que usa base;
- `activity schema` JSON refleja enums, requeridos y mutabilidad;
- `activity add` crea una actividad valida y aplica el status por defecto;
- `activity add` rechaza enum invalido, titulo vacio y titulo de mas de 200 caracteres;
- `activity list` ordena por `created_at ASC` e `id ASC`, filtra, pagina y produce un arreglo JSON valido;
- `activity count` cuenta todos los registros o los que coinciden con filtros JSON;
- `activity get` encuentra registros y devuelve not found correctamente;
- `activity update` cambia solo campos enviados y conserva `id` y `created_at`;
- `activity delete` elimina registros y maneja not found;
- las respuestas de error JSON son validas, estables y se escriben en `stderr`.

## Criterio de aceptacion futuro

Una implementacion del MVP se considerara terminada cuando el siguiente flujo
funcione desde la raiz de un proyecto:

```bash
dbox init

dbox activity schema

dbox activity add --json '{"type":"implementation","title":"Primera actividad"}'

dbox activity list

dbox activity get 1

dbox activity update 1 --json '{"status":"completed"}'

dbox activity delete 1
```

El mismo flujo JSON es valido para scripts y agentes:

```bash
dbox activity schema

dbox activity add --json '{"type":"research","title":"Prueba"}'

dbox activity count --json '{"status":"completed"}'

dbox activity list --json '{"status":"completed"}' --skip 0 --take 10
```

La validacion final debera incluir compilacion, ejecucion de todas las pruebas
y una comprobacion manual de estos flujos en directorios temporales separados.
