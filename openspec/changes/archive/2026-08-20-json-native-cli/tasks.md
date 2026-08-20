## 1. Contrato y documentacion

- [x] 1.1 Actualizar `PROJECT.md` con la CLI JSON por defecto, los payloads de `--json`, la ayuda textual, el comando `count`, la paginacion de `list` y el orden por `created_at ASC`, `id ASC`.
- [x] 1.2 Ajustar los ejemplos de uso y los criterios de aceptacion de `PROJECT.md` para eliminar las opciones y aliases retirados.

## 2. Ejecucion y salida de CLI

- [x] 2.1 Eliminar la seleccion de formato `--output`, el renderizado exitoso y de errores en texto, y las rutas auxiliares de deteccion de formato.
- [x] 2.2 Hacer que las respuestas exitosas y los errores operacionales y de sintaxis se serialicen siempre como un unico valor JSON en el stream correspondiente.
- [x] 2.3 Conservar la salida humana nativa de `--help` sin ejecutar operaciones de base de datos.
- [x] 2.4 Retirar la normalizacion y aceptacion de `activity --schema`; conservar solo el subcomando `activity schema` sin opcion `--json`.

## 3. Payloads y comandos activity

- [x] 3.1 Cambiar `activity add` para exigir un objeto `--json` y retirar las opciones individuales de campos.
- [x] 3.2 Cambiar `activity update <id>` para exigir un objeto `--json`, conservar el ID posicional y rechazar IDs o campos generados dentro del payload.
- [x] 3.3 Implementar el parseo y la validacion compartidos para el payload opcional de filtros `type` y `status` usado por `list` y `count`.
- [x] 3.4 Agregar `--skip` y `--take` opcionales a `activity list`, con validacion de enteros no negativos.
- [x] 3.5 Agregar `activity count` al grupo del catalogo y devolver su respuesta JSON estable `{ "count": <integer> }`.

## 4. Consultas de actividades

- [x] 4.1 Reestructurar las consultas del repositorio para compartir filtros entre listado y conteo.
- [x] 4.2 Ordenar los listados por `created_at ASC` y luego `id ASC` antes de aplicar `skip` y `take`.
- [x] 4.3 Implementar el conteo sin paginacion y sin modificar el esquema SQLite ni las migraciones.

## 5. Pruebas y verificacion

- [x] 5.1 Actualizar las pruebas de ayuda, inicializacion, schema, CRUD, errores e integracion al contrato JSON por defecto y las opciones eliminadas.
- [x] 5.2 Agregar pruebas para payloads JSON obligatorios en `add` y `update`, filtros opcionales de `list`, y rechazo de payloads u opciones invalidadas.
- [x] 5.3 Agregar pruebas para `count` sin filtros y filtrado, el orden por fecha con desempate por ID, y las ventanas `skip`/`take`.
- [x] 5.4 Ejecutar `dotnet build Dbox.sln` y `dotnet test Dbox.sln`.
