## 1. Entrada y consulta compartidas

- [ ] 1.1 Incorporar `--json-file <path>` y `--json-file -` como alternativa exclusiva de `--json` en `activity add`, `list`, `count` y `update`, con lectura UTF-8 y errores JSON de validación deterministas.
- [ ] 1.2 Centralizar la selección, lectura y análisis de la fuente JSON para conservar la validación de objetos, propiedades y campos entre entrada inline, archivo y stdin.
- [ ] 1.3 Definir y validar el filtro de actividad compartido con `type`, `status`, `area`, `source`, `effort`, `created_from`, `created_to`, `title` y `description`, incluidos rangos UTC inclusivos y búsqueda parcial insensible a mayúsculas.
- [ ] 1.4 Aplicar el filtro validado mediante una consulta EF Core compartida para que `list` y `count` tengan semántica de coincidencia idéntica.

## 2. Contrato de comandos

- [ ] 2.1 Cambiar `activity list` para aplicar `skip` predeterminado 0, `take` predeterminado 100, `--all` sin límite y la incompatibilidad validada entre `--all` y `--take`.
- [ ] 2.2 Sustituir la respuesta de arreglo de `activity list` por el envelope `items` y `pagination`, calculando `skip`, `take`, `total` y `has_more` contra la consulta filtrada y ordenada.
- [ ] 2.3 Actualizar `activity count` para aceptar la fuente JSON alternativa y todos los filtros compartidos sin paginación.
- [ ] 2.4 Actualizar la ayuda de los comandos para describir `--json-file`, las opciones de consulta y `--all`.

## 3. Pruebas de contrato

- [ ] 3.1 Cubrir `--json-file` desde archivo y stdin, la exclusión mutua con `--json`, archivos ilegibles y JSON inválido en cada comando de actividad que acepta payloads.
- [ ] 3.2 Cubrir filtros individuales y combinados de lista y conteo, límites UTC inclusivos, rango invertido inválido y búsqueda parcial insensible a mayúsculas.
- [ ] 3.3 Cubrir el envelope breaking de `activity list`, su límite predeterminado de 100, `--skip`/`--take`, `--all`, `total`, `has_more` y resultados vacíos.
- [ ] 3.4 Cubrir los errores deterministas de paginación y filtros inválidos, incluida la combinación `--all` con `--take`.

## 4. Verificación

- [ ] 4.1 Ejecutar `dotnet build Dbox.sln`.
- [ ] 4.2 Ejecutar `dotnet test Dbox.sln`.
