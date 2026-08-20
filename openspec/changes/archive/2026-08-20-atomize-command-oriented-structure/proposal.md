## Why

La CLI ya expone una jerarquia de comandos orientada a catalogos, pero la
implementacion no refleja esa jerarquia: `DboxCli.cs` concentra la composicion,
las opciones, la ejecucion y los seis comandos de `activity`, mientras que las
pruebas se concentran en un unico archivo de CLI. Esto dificulta localizar un
flujo, aislar cambios y extender la herramienta con nuevos catalogos.

## What Changes

- Reorganizar `src/Dbox` para que la estructura de comandos refleje `init`,
  `activity` y cada operacion hija (`schema`, `add`, `list`, `get`, `update` y
  `delete`).
- Extraer la composicion de la raiz, la composicion del catalogo `activity`, un
  ejecutor comun y una unidad de comando por cada comando hoja.
- Separar los tipos publicos actuales en archivos atomizados, manteniendo las
  reglas de `ActivitySchema` como fuente unica de verdad.
- Mover el repositorio especifico de actividades junto al soporte del catalogo,
  manteniendo en `Database` solo la infraestructura compartida, el contexto,
  el locator, la fabrica y las migraciones.
- Alinear los namespaces de los comandos con sus nuevas carpetas y conservar
  `Dbox.Activities` para evitar una alteracion innecesaria de la identidad CLR
  usada por EF Core y sus migraciones.
- Dividir `tests/Dbox.Tests` en carpetas espejo de la CLI, con ubicaciones
  explicitas para integracion, base de datos y utilidades de prueba, sin crear
  proyectos de test adicionales.
- Mantener sin cambios el contrato observable: comandos, aliases validos y
  rechazados, formatos de salida, errores, exit codes, descubrimiento de base,
  migraciones y esquema SQLite.

## Capabilities

### New Capabilities

No se introducen capacidades observables nuevas.

### Modified Capabilities

No se modifican requisitos de comportamiento existentes. Este cambio es un
refactor estructural y declara `skip_specs: true` en `.openspec.yaml`.

## Impact

- Afecta la organizacion de archivos, namespaces y clases en `src/Dbox`.
- Afecta la composicion de `System.CommandLine` sin cambiar su arbol publico.
- Afecta la ubicacion del repositorio de `activity` y la dependencia interna de
  los comandos respecto a la infraestructura de base de datos.
- Afecta la organizacion y nombres de namespaces de las pruebas en
  `tests/Dbox.Tests`.
- Requiere revisar las referencias CLR del modelo EF Core y conservar intactos
  los archivos generados de migraciones salvo que una validacion demuestre que
  un cambio de namespace del modelo exige una migracion nueva.
- No agrega dependencias, tablas, comandos, aliases de compatibilidad ni
  proyectos .NET adicionales.
