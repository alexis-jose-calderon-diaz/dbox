## 1. Modelo y migracion

- [x] 1.1 Extender `Activity`, `ActivityView` y `ActivitySchema` con `updated_at` y `version`, incluidas sus reglas de generacion, mutabilidad y serializacion UTC.
- [x] 1.2 Actualizar `DboxDbContext` para configurar las dos columnas requeridas y `version` como token de concurrencia, sin duplicar reglas de `ActivitySchema`.
- [x] 1.3 Generar con `dotnet ef` una migracion EF Core nueva y su model snapshot para agregar `updated_at` y `version`, con valores por defecto aptos para las actividades existentes.
- [x] 1.4 Ajustar la creacion de actividades para capturar una sola hora UTC para ambos timestamps e inicializar la version en `1`.

## 2. Validacion y persistencia

- [x] 2.1 Extender el parser y la validacion de update para exigir `version` positiva como precondicion y seguir rechazando los campos generados no permitidos.
- [x] 2.2 Implementar la actualizacion EF Core condicional por ID y version, con incremento atomico de version, nuevo `updated_at`, distincion de recurso ausente y mapeo de concurrencia a `conflict_error`.
- [x] 2.3 Incorporar los tipos y parser de registros portables completos para JSON y JSONL, incluidos formato estricto, propiedades desconocidas, timestamps UTC e IDs/versiones positivos.
- [x] 2.4 Implementar en `ActivityRepository` la exportacion ordenada y la importacion serializable, validada previamente y atomica, preservando los campos del registro y mapeando las colisiones de ID.
- [x] 2.5 Extender `CliError`, `ExitCodes` y la ejecucion compartida para emitir `conflict_error` e `io_error` con los exit codes acordados.

## 3. Comandos y salida

- [x] 3.1 Agregar y registrar `dbox activity export [--format json|jsonl]`, con JSON como predeterminado, JSONL sin salida auxiliar y migraciones previas.
- [x] 3.2 Agregar y registrar `dbox activity import --file <path> --format <json|jsonl>`, con respuesta JSON de conteo/formato y mapeo de errores de lectura, validacion, conflicto y base de datos.
- [x] 3.3 Verificar que `activity schema`, add, list, get y update devuelven `updated_at` y `version`, y que la ayuda expone los dos nuevos comandos y sus opciones.

## 4. Pruebas y verificacion

- [x] 4.1 Ampliar las pruebas de schema y CRUD para cubrir los campos generados nuevos, version inicial, incremento, timestamp de actualizacion, version ausente/invalida y actualizacion obsoleta sin sobrescritura.
- [x] 4.2 Agregar pruebas aisladas de export JSON y JSONL, catalogo vacio, orden estable y rechazo de formato no admitido.
- [x] 4.3 Agregar pruebas aisladas de import JSON y JSONL que cubran round trip, datos malformados o incompletos, archivo ilegible, IDs repetidos o existentes, rollback de escritura y preservacion de todos los campos.
- [x] 4.4 Agregar una prueba de migracion de una base existente que confirme los valores iniciales de `updated_at` y `version` y que los comandos migran antes de operar.
- [x] 4.5 Ejecutar `dotnet build Dbox.sln` y `dotnet test Dbox.sln` y corregir cualquier regresion resultante.
