## Why

`dbox` tiene definido su contrato funcional, pero el repositorio aun no tiene
una CLI ejecutable ni una base local que lo implemente. Este cambio convierte el
contrato del MVP en una herramienta usable desde terminal, con comportamiento
determinista para personas, scripts y agentes.

## What Changes

- Crear la aplicacion CLI .NET 10 y su proyecto de pruebas xUnit.
- Implementar una base SQLite independiente por proyecto, con descubrimiento
  local y migraciones versionadas de EF Core.
- Implementar el contrato fijo de `activity`, su validacion y el comando
  `schema` en formatos humano y JSON.
- Implementar `init`, `add`, `list`, `get`, `update` y `delete`, incluyendo sus
  entradas por opciones o JSON, filtros y reglas de mutabilidad.
- Estabilizar salidas, errores en `stderr` y exit codes para automatizacion.
- Cubrir el flujo con pruebas temporales de locator, migraciones, contrato,
  CRUD, aislamiento entre proyectos y respuestas JSON.

## Capabilities

### New Capabilities

- `project-database`: inicializacion, descubrimiento, migraciones e
  independencia de las bases locales por proyecto.
- `activity-contract`: entidad fija `activity`, metadatos compartidos,
  validacion y exposicion del contrato mediante `schema`.
- `activity-crud`: creacion, listado, consulta, actualizacion y eliminacion de
  actividades mediante opciones de terminal o JSON.
- `cli-contract`: formatos de salida, errores, `stderr`, alias y exit codes
  estables para la CLI.

### Modified Capabilities

None.

## Impact

- Agrega la aplicacion en `src/Dbox/` y las pruebas en `tests/Dbox.Tests/`.
- Agrega dependencias de `System.CommandLine`, EF Core SQLite y xUnit, ademas
  de las migraciones internas generadas con `dotnet-ef`.
- Define el ejecutable `dbox` como contrato publico local; no agrega servicios
  remotos, API HTTP, MCP ni una base global.
- Introduce `.dbox/data.db` como artefacto de ejecucion local que no debe entrar
  al control de versiones.
