## 1. Documentacion y linea base

- [x] 1.1 Actualizar la seccion de arquitectura futura de `PROJECT.md` con la estructura espejo de `Commands`, `Activities`, `Database`, `Output` y `tests/Dbox.Tests`.
- [x] 1.2 Ejecutar `dotnet build Dbox.sln` y `dotnet test Dbox.sln` antes de mover codigo, registrando la linea base de compilacion y las 21 pruebas existentes.

## 2. Composicion y ejecucion comun de la CLI

- [x] 2.1 Crear el ejecutor comun de comandos a partir de `CommandRuntime.RunAsync`, preservando el parseo de `--output`, la escritura de resultados, la traduccion de `CliException`, la cancelacion y los exit codes.
- [x] 2.2 Crear `Commands/Root/RootCommand.cs` para componer la raiz, la opcion recursiva `--output`, `init` y el grupo `activity` sin exponer comandos de catalogo en la raiz.
- [x] 2.3 Mantener `DboxCli.InvokeAsync` como punto de entrada y trasladar a la composicion nueva la deteccion de sintaxis invalida y de formato de error sin cambiar sus mensajes.
- [x] 2.4 Eliminar de `DboxCli` la definicion centralizada de opciones, argumentos y acciones una vez que todas las ramas nuevas esten conectadas.

## 3. Comandos raiz y de activity

- [x] 3.1 Crear `Commands/Init/InitCommand.cs` con las opciones y la accion actuales de `dbox init`, conservando sus tres estados de respuesta.
- [x] 3.2 Crear `Commands/Activity/ActivityCommand.cs` para registrar `schema`, `add`, `list`, `get`, `update` y `delete`, y limitar a ese grupo la normalizacion del alias `--schema`.
- [x] 3.3 Crear `Commands/Activity/Schema/SchemaCommand.cs` y conservar la salida humana, `--json`, la resolucion de base y la aplicacion previa de migraciones.
- [x] 3.4 Crear `Commands/Activity/Add/AddCommand.cs` con las opciones, entradas JSON o por opciones, validacion previa y persistencia actuales.
- [x] 3.5 Crear `Commands/Activity/List/ListCommand.cs` con filtros, validacion de enums, orden `id DESC` y formatos de salida actuales.
- [x] 3.6 Crear `Commands/Activity/Get/GetCommand.cs` con el argumento `id`, la respuesta completa y el error de recurso inexistente actuales.
- [x] 3.7 Crear `Commands/Activity/Update/UpdateCommand.cs` con actualizaciones parciales, entrada JSON, limpieza de `description` y validacion actuales.
- [x] 3.8 Crear `Commands/Activity/Delete/DeleteCommand.cs` con el argumento `id`, la respuesta de eliminacion y el error de recurso inexistente actuales.
- [x] 3.9 Revisar que cada comando hoja defina solo sus opciones, argumentos y accion, y que las dependencias se pasen explicitamente sin introducir DI compleja o handlers genericos.

## 4. Soporte del catalogo y salida

- [x] 4.1 Separar `Activity`, `ActivityView`, `ActivityCreateInput` y `ActivityUpdateInput` en archivos individuales dentro de `Activities`, conservando el namespace `Dbox.Activities`.
- [x] 4.2 Separar las definiciones y documentos publicos de `ActivitySchema` en archivos atomizados sin duplicar enums, defaults, mutabilidad ni limites.
- [x] 4.3 Separar `ValidationIssue`, `ValidationResult` e `InputResult<T>` y conservar el comportamiento de `ActivityValidator` y `ActivityInputParser`.
- [x] 4.4 Mover `ActivityRepository` desde `Database` a `Activities`, actualizar `DboxDbContext` y los comandos, y conservar consultas, orden, filtros y operaciones de persistencia.
- [x] 4.5 Separar `InitResponse` y `DeleteResponse` en archivos individuales bajo `Output`, manteniendo el contrato JSON y las referencias del writer y de la base.
- [x] 4.6 Mantener `OutputWriter` y `OutputFormat` como salida comun, verificando que la serializacion JSON, la salida textual y los errores en `stderr` no cambien.
- [x] 4.7 Confirmar que `Dbox.Activities.Activity`, `Dbox.Database.Migrations` y los archivos generados de migraciones no cambian de identidad ni se editan manualmente.

## 5. Namespaces y estructura de pruebas

- [x] 5.1 Alinear los namespaces de `RootCommand`, `InitCommand`, `ActivityCommand` y cada comando hoja con sus carpetas bajo `Dbox.Commands`.
- [x] 5.2 Crear `tests/Dbox.Tests/Support` y separar `TestProject` y `CliResult`, conservando directorios temporales independientes por caso.
- [x] 5.3 Mover `DboxLocatorTests` a `tests/Dbox.Tests/Database` y alinear su namespace con la carpeta.
- [x] 5.4 Separar `DboxCliTests` en clases y archivos bajo `Commands/Root`, `Commands/Init` y cada comando de `Commands/Activity`.
- [x] 5.5 Trasladar los flujos que combinan varios comandos a `tests/Dbox.Tests/Integration` sin reducir la cobertura de aislamiento, migraciones, ciclo CRUD ni errores.
- [x] 5.6 Revisar todos los `using`, namespaces y referencias de pruebas, manteniendo un unico `Dbox.Tests.csproj`.

## 6. Verificacion funcional y estructural

- [x] 6.1 Ejecutar `dotnet build Dbox.sln` y corregir referencias, namespaces o archivos incluidos que fallen.
- [x] 6.2 Ejecutar `dotnet test Dbox.sln` y confirmar que toda la cobertura existente sigue pasando en su nueva ubicacion.
- [x] 6.3 Verificar manualmente `dbox --help`, `dbox activity --help`, el alias `dbox activity --schema` y el rechazo de comandos planos y `dbox --schema`.
- [x] 6.4 Verificar manualmente salidas text/json, errores en `stderr`, exit codes, descubrimiento de bases, inicializacion anidada y migraciones sobre directorios temporales.
- [x] 6.5 Revisar el diff final para confirmar que no se agregaron dependencias, migraciones, tablas, proyectos de prueba ni cambios funcionales no previstos.
