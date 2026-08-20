## 1. Contrato y documentacion

- [x] 1.1 Actualizar `PROJECT.md` para definir la raiz como infraestructura compartida, conservar `dbox init` como inicializador unico y describir `activity` como el primer catalogo.
- [x] 1.2 Reemplazar en `PROJECT.md` las rutas, aliases, mensajes de orientacion y flujos de aceptacion de actividad por sus equivalentes `dbox activity`, y retirar la documentacion de rutas planas.

## 2. Composicion de la CLI

- [x] 2.1 Reorganizar la composicion de `System.CommandLine` para conservar solo `init` y el grupo `activity` en la raiz, con `schema`, `add`, `list`, `get`, `update` y `delete` como hijos de ese grupo.
- [x] 2.2 Limitar la normalizacion de `--schema` y la deteccion JSON de errores al grupo `dbox activity`, de modo que los aliases y comandos planos retirados produzcan errores de sintaxis sin acceder a la base.
- [x] 2.3 Conservar el alcance recursivo de `--output` y los handlers, validacion, salida, localizacion y migracion existentes para las operaciones anidadas.

## 3. Pruebas

- [x] 3.1 Migrar las pruebas de operaciones y schema de actividades a las rutas `dbox activity ...`, preservando la cobertura de entradas, salidas, errores, descubrimiento y migraciones.
- [x] 3.2 Agregar pruebas para la ayuda raiz y de `activity`, el alias `dbox activity --schema`, y el rechazo con error de validacion de todos los comandos y alias planos retirados.

## 4. Verificacion

- [x] 4.1 Ejecutar `dotnet build Dbox.sln`.
- [x] 4.2 Ejecutar `dotnet test Dbox.sln`.
