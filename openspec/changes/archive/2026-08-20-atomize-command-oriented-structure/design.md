## Context

La aplicacion actual compone toda la CLI y ejecuta todos sus flujos desde
`Dbox.Cli.DboxCli`. El modelo de `activity`, sus reglas y su repositorio estan
separados parcialmente, pero `ActivityRepository` permanece bajo
`Database`, y las pruebas de comandos estan concentradas en
`tests/Dbox.Tests/DboxCliTests.cs`.

El SDK de .NET incluye automaticamente los archivos `.cs`, por lo que la
reorganizacion no requiere listas de compilacion nuevas en los `.csproj`. El
proyecto debe conservar composicion explicita, sin contenedor DI complejo,
repositorios genericos ni capas `Domain/Application/Infrastructure/Presentation`.
EF Core mantiene migraciones generadas bajo `Database/Migrations`, y
`Dbox.Activities.Activity` aparece como identidad CLR en el model snapshot.

## Goals / Non-Goals

**Goals:**

- Hacer que la estructura de comandos sea visible directamente en el arbol de
  `src` y `tests`.
- Reducir `DboxCli` a invocacion y composicion, extrayendo la ejecucion comun y
  una unidad de construccion por comando.
- Mantener limites claros entre comandos, soporte del catalogo `activity` e
  infraestructura de base de datos.
- Alinear los namespaces de los comandos y las pruebas con sus carpetas.
- Mantener un unico proyecto de pruebas y un unico mecanismo de salida comun.
- Verificar que el refactor no modifica el contrato observable ni el esquema.

**Non-Goals:**

- Cambiar comandos, aliases, opciones, formatos, errores, exit codes o ayuda
  observable.
- Agregar catalogos, tablas, migraciones o dependencias nuevas.
- Crear un framework de handlers, mediator, interfaces genericas o un
  contenedor de inyeccion de dependencias.
- Cambiar el namespace de `Activity` o de otros tipos que formen parte de la
  identidad del modelo EF Core solo para forzar una simetria de carpetas.
- Separar `OutputWriter` en renderizadores independientes en esta iteracion.

## Decisions

### La CLI se organiza como un espejo de la jerarquia publica

Se crearan unidades de composicion para la raiz, `init`, el grupo `activity` y
cada comando hoja:

```text
Commands/
├── Root/RootCommand.cs
├── Init/InitCommand.cs
└── Activity/
    ├── ActivityCommand.cs
    ├── Schema/SchemaCommand.cs
    ├── Add/AddCommand.cs
    ├── List/ListCommand.cs
    ├── Get/GetCommand.cs
    ├── Update/UpdateCommand.cs
    └── Delete/DeleteCommand.cs
```

Cada clase construira su `System.CommandLine.Command`, registrara solo sus
opciones y argumentos y conectara su accion. `DboxCli` conservara la entrada
publica `InvokeAsync`, la normalizacion previa necesaria y el parseo inicial,
pero no contendra los flujos de negocio de los comandos.

Alternativa considerada: mantener un `CommandRuntime` central y solo mover
archivos. Se descarta porque produciria una estructura cosmetica: cada nuevo
comando seguiria aumentando el mismo punto de acoplamiento.

### La ejecucion transversal se extrae, pero la composicion sigue siendo explicita

La logica comun actualmente contenida en `CommandRuntime.RunAsync` se movera a
un ejecutor pequeno de `Dbox.Cli`. Ese ejecutor sera responsable de interpretar
`--output`, ejecutar la accion, serializar el resultado, traducir
`CliException` y preservar los codigos de salida.

Las dependencias se pasaran desde la composicion de la raiz a los comandos; no
se introducira descubrimiento de servicios ni registro dinamico. El grupo
`activity` recibira las mismas instancias explicitas de `DboxDatabase`,
`ActivityRepository` y `OutputWriter` que usa actualmente el runtime.

### El soporte de activity permanece compartido por sus comandos

Los tipos que son usados por mas de un comando no se duplicaran dentro de las
carpetas hoja. `ActivitySchema` continuara siendo la fuente unica de enums,
limites, defaults, mutabilidad y metadatos. `Activity`, `ActivityView`, los
modelos de entrada, el parser, la validacion y `ActivityRepository` quedaran
en el modulo `Activities/`, con un tipo publico por archivo cuando sea viable.

`ActivityRepository` se movera desde `Database/` a `Activities/` porque es
especifico del catalogo. `Database/` conservara `DboxDatabase`,
`DboxDbContext`, `DboxDbContextFactory`, `DboxLocation`, `DboxLocator` y
`Migrations/` como infraestructura de proyecto compartida.

Alternativa considerada: colocar la entidad y el repositorio bajo
`Commands/Activity/Support`. Se descarta para la entidad porque cambiaria la
identidad CLR `Dbox.Activities.Activity` que usa EF Core; el modulo
`Activities/` ya representa correctamente el soporte del catalogo y permite
alinear sus namespaces con su carpeta.

### Los namespaces se alinean solo donde no afectan la identidad de EF Core

Los namespaces de composicion y comandos seguiran el arbol fisico:

```text
Dbox.Commands.Root
Dbox.Commands.Init
Dbox.Commands.Activity
Dbox.Commands.Activity.Schema
Dbox.Commands.Activity.Add
...
```

Los namespaces transversales seguiran siendo `Dbox.Cli`, `Dbox.Output` y
`Dbox.Database`; el soporte del catalogo continuara en `Dbox.Activities`.
Los archivos generados de `Database/Migrations` no se editaran manualmente.

Si una comprobacion de EF Core demostrara que algun namespace de modelo debe
cambiar, se detendra el movimiento estructural de ese tipo y se tratara como
una decision de migracion separada, no como una consecuencia silenciosa del
refactor.

### OutputWriter permanece comun en la primera iteracion

`OutputFormat`, `OutputFormatParser` y `OutputWriter` permaneceran bajo
`Output/`. Se separaran los modelos publicos en archivos individuales cuando
corresponda, pero el `switch` de salida textual no se reemplazara por una
jerarquia de renderizadores ni interfaces.

Esto mantiene el riesgo bajo y conserva exactamente la serializacion existente.
La dependencia de `OutputWriter` hacia tipos de activity queda documentada como
un limite conocido para una futura extension, no como motivo para introducir
una abstraccion prematura.

### Las pruebas siguen un unico proyecto con carpetas espejo

`tests/Dbox.Tests` conservara su `.csproj` y se organizara así:

```text
Commands/
├── Root/
├── Init/
└── Activity/
    ├── Schema/
    ├── Add/
    ├── List/
    ├── Get/
    ├── Update/
    └── Delete/
Integration/
Database/
Support/
```

Los escenarios que atraviesan varios comandos permaneceran en `Integration`.
Los tests del locator y de errores de persistencia permaneceran bajo
`Database`; `TestProject` y `CliResult` quedaran en `Support`. Cada prueba
seguira creando su propio directorio temporal y su propia base.

Alternativa considerada: un proyecto de tests por comando. Se descarta porque
no aporta un limite de ejecucion necesario, aumenta referencias y complica la
infraestructura temporal compartida.

## Risks / Trade-offs

- [Una extraccion incompleta puede dejar un runtime central oculto] -> Cada
  comando hoja debe poseer su definicion de opciones, argumentos y accion; el
  ejecutor comun solo maneja preocupaciones transversales.
- [Cambiar el namespace de `Activity` puede alterar el model snapshot de EF
  Core] -> Conservar `Dbox.Activities`, no editar migraciones generadas y
  ejecutar build, tests y comprobaciones de migraciones antes de aceptar el
  movimiento.
- [Separar pruebas puede perder cobertura de escenarios combinados] -> Mantener
  una suite `Integration` y trasladar los flujos multi-comando completos, no
  reducirlos a tests unitarios de cada hoja.
- [La salida comun conserva conocimiento de activity] -> Mantener el writer
  sin cambios funcionales en esta iteracion y registrar la separacion de
  renderers como una futura decision solo si aparece un segundo catalogo.
- [Los cambios de namespaces pueden dejar referencias obsoletas] -> Realizar
  los movimientos por grupos, compilar tras cada grupo y usar busqueda de
  referencias antes de eliminar archivos antiguos.
- [La estructura nueva puede divergir de `PROJECT.md`] -> Actualizar la
  seccion de arquitectura futura en la misma entrega, sin modificar el
  contrato funcional de la CLI.

## Migration Plan

1. Actualizar la documentacion tecnica de `PROJECT.md` con el arbol aprobado.
2. Crear la estructura de comandos y extraer la composicion de raiz, `init`,
   `activity` y sus comandos hoja, conservando el comportamiento actual.
3. Extraer el ejecutor comun y trasladar los tipos publicos a archivos
   individuales sin cambiar los contratos de salida.
4. Mover `ActivityRepository` al modulo `Activities` y revisar referencias del
   contexto y de los comandos.
5. Reorganizar los namespaces de comandos y dividir las pruebas en carpetas
   espejo, conservando los escenarios de integracion.
6. Ejecutar `dotnet build Dbox.sln` y `dotnet test Dbox.sln`.
7. Validar manualmente la ayuda, aliases, salidas text/json, errores y los
   flujos de base temporal definidos en `PROJECT.md`.

El rollback es de codigo y no requiere migracion de datos: si la verificacion
falla, se revierte el refactor de archivos y namespaces sin modificar
`.dbox/data.db` ni los archivos de migracion aplicados.
