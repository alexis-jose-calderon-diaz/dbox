## Context

El repositorio contiene el contrato de `dbox` en `PROJECT.md`, pero no tiene
codigo de aplicacion ni proyectos .NET. La propuesta y las specs de este cambio
definen el comportamiento del MVP; este documento define como implementarlo sin
agregar capas generales ni servicios externos.

## Goals / Non-Goals

**Goals:**

- Componer una CLI pequena y explicita cuyo flujo pueda seguirse desde
  `Program.cs`.
- Centralizar la ubicacion de la base y la aplicacion de migraciones antes de
  cada operacion que usa datos.
- Usar una definicion unica de `activity` para validacion, schema, salida y
  configuracion de EF Core.
- Mantener salidas y errores deterministas y probarlos con directorios
  temporales aislados.

**Non-Goals:**

- Crear una arquitectura por capas, un contenedor DI complejo, repositorios
  genericos o un servicio remoto.
- Crear un sistema de entidades dinamicas, consultas SQL arbitrarias o una
  base global.
- Resolver la distribucion como paquete global; el MVP entrega un ejecutable
  local y puede ejecutarse desde el proyecto o una publicacion local.

## Decisions

### Estructura y composicion

La solucion usara `Dbox.sln`, `src/Dbox/` y `tests/Dbox.Tests/`. `Program.cs`
construira explicitamente el root command, el locator, el contexto, el
repositorio especifico de actividades y los escritores de salida. Se descarta
una jerarquia `Domain/Application/Infrastructure/Presentation` porque el
contrato pide una arquitectura pequena y la composicion explicita hace visibles
las dependencias reales.

### Resolucion de proyectos

`DboxLocator` recibira un directorio inicial normalizado y devolvera la ruta
absoluta del proyecto y de `data.db`. El comando `init` no usara el recorrido
ascendente: siempre construira `.dbox/data.db` a partir del directorio actual.
Los demas comandos usaran el mismo locator, deteniendose en el primer `.dbox`,
incluso si carece de `data.db`. Recibir el directorio como entrada permite
probar todos los casos sin cambiar el directorio de trabajo global del proceso.

### Persistencia y migraciones

`DboxDbContext` recibira la ruta ya resuelta y configurara SQLite mediante
`Microsoft.EntityFrameworkCore.Sqlite`. La factory de `dotnet-ef` usara una
fuente SQLite de diseño no persistente, separada de las bases de usuario; el
runtime nunca inferira la ruta desde la factory. Las migraciones se generaran
con `dotnet-ef`, se versionaran junto al contexto y se aplicaran con
`Database.MigrateAsync()` antes de la operacion principal.

Para distinguir los resultados de `init`, primero se observara si `data.db`
existia. Una ruta nueva produce `initialized`; una ruta existente se revisa
contra las migraciones pendientes y produce `migrated` o
`already_initialized`. Ningun caso elimina, reemplaza ni trunca la base.
No se usaran `Microsoft.Data.Sqlite`, `SqliteConnection`, `SqliteCommand` ni
SQL CRUD escrito a mano.

### Modelo y fuente unica de reglas

`ActivitySchema` sera el registro compartido de nombres publicos, tipos,
campos generados, mutabilidad, enums, default de `status` y limite de `title`.
El validador, la forma JSON de `schema`, la configuracion de la entidad y los
modelos de entrada consultaran esa definicion en vez de repetir constantes.
El repositorio sera especifico para `activity` y usara consultas LINQ de EF
Core.

### Tiempo y salida de datos

La fecha se generara en UTC y se mantendra como un valor de fecha interno; el
writer de salida controlara el formato publico exacto
`YYYY-MM-DDTHH:mm:ssZ`, sin aceptar la fecha del usuario. El mismo writer
recibira el formato `text` o `json`; las respuestas exitosas van a `stdout` y
los errores clasificados van a `stderr`. Los handlers devolveran los exit
codes contractuales en vez de usar excepciones de validacion como flujo normal.

### Entradas por opciones y JSON

Los comandos `add` y `update` tendran modelos de entrada separados para
distinguir propiedades ausentes de propiedades presentes con valor `null`.
El parser rechazara la mezcla de `--json` con opciones de campos y el lector
JSON rechazara propiedades desconocidas o de solo lectura. `description: null`
sera una operacion explicita solo en el update JSON; la ausencia de la
propiedad no cambiara el valor almacenado.

### Errores de CLI

El root command convertira errores de sintaxis, validacion, recurso, base e
inesperados al envelope JSON definido por la spec cuando se seleccione JSON.
Los mensajes humanos conservaran las cadenas publicas del contrato. Ningun
mensaje de migracion o diagnostico se mezclara con la salida exitosa de
`schema`, `add`, `list`, `get`, `update` o `delete`.

### Pruebas

Las pruebas de locator usaran rutas temporales construidas como datos de
entrada. Las pruebas de integracion crearan un directorio y una base SQLite
independientes por caso, invocaran los comandos con salidas capturadas y
comprobaran `stdout`, `stderr`, exit code y estado persistido. Se cubriran
explicitamente proyectos anidados, `.dbox` incompletos, migraciones pendientes,
aislamiento y la matriz minima de `PROJECT.md`.

La raiz ignorara `bin/`, `obj/`, `.dbox/` y artefactos de pruebas para que las
pruebas locales no contaminen el control de versiones.

## Risks / Trade-offs

- [La API de errores predeterminada de `System.CommandLine` puede escribir en
  el canal o formato equivocado] -> Centralizar el manejo de parseo y errores
  antes de conectar los handlers y probar cada exit code.
- [Una factory de EF Core puede crear accidentalmente una base en el
  repositorio] -> Usar una fuente no persistente exclusiva de diseño y cubrir
  la generacion de migraciones sin tocar `.dbox`.
- [La configuracion de EF, el schema y el validador pueden divergir] -> Hacer
  que todos consulten `ActivitySchema` y agregar pruebas que comparen metadata
  y salida JSON.
- [Las pruebas que cambian el directorio actual pueden interferirse entre si]
  -> Inyectar el directorio inicial del locator y evitar `SetCurrentDirectory`
  en las pruebas unitarias.
- [La precision o zona horaria de `created_at` puede variar entre proveedor y
  salida] -> Generar UTC en un solo punto y verificar el formato publico exacto
  en pruebas de CLI.
- [El alcance completo del MVP puede producir un cambio dificil de revisar]
  -> Implementar en cortes ordenados: bootstrap, `init`/`schema`, CRUD y
  endurecimiento del contrato, manteniendo estas specs como una sola fuente.

## Migration Plan

1. Crear la solucion, proyectos y `.gitignore` antes de generar artefactos
   locales.
2. Generar la migracion inicial con `dotnet-ef` y verificar `init` en un
   directorio temporal.
3. Implementar los comandos por los cortes definidos en `tasks.md`, aplicando
   migraciones antes de cada operacion.
4. Ejecutar compilacion, todas las pruebas y los flujos manuales del contrato
   en directorios temporales separados.

No existe una base previa que migrar en este repositorio. Si una migracion
futura falla, el comando informara el error sin intentar recuperar mediante
SQL manual ni eliminar la base; cualquier cambio posterior del esquema se
representara con una nueva migracion.
