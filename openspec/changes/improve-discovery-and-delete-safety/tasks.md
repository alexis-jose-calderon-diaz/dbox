## 1. Descubrimiento y contexto de proyecto

- [x] 1.1 Extender el resultado centralizado de descubrimiento para conservar el `cwd` absoluto, las rutas de proyecto resueltas y los estados `found`, `incomplete` y `not_found`, manteniendo el error de base para los comandos que la requieren.
- [x] 1.2 Agregar el comando raíz `dbox context` y su respuesta JSON estable con rutas absolutas o valores `null`, sin abrir, crear ni migrar una base.
- [x] 1.3 Cubrir `context` en directorios temporales para los estados found, incomplete y not_found, incluido el límite de una `.dbox` incompleta ante una base ancestro.

## 2. Contrato instalado y eliminación segura

- [x] 2.1 Desacoplar `dbox activity schema` del localizador y de la migración para que renderice el contrato instalado sin requerir `.dbox/data.db`.
- [x] 2.2 Agregar `--yes` y `--dry-run` a `dbox activity delete`, exigir confirmación para persistir y devolver la previsualización no mutante cuando aplica.
- [x] 2.3 Mantener `--dry-run` libre de migraciones y escrituras, con `--dry-run` prevaleciendo sobre `--yes`, y conservar el resultado resource-not-found para IDs ausentes.
- [x] 2.4 Añadir pruebas de schema sin base y con migraciones pendientes, y de delete sin confirmación, confirmado, dry-run existente, dry-run ausente y ambos flags.

## 3. Permisos de inicialización Linux

- [x] 3.1 En `init`, aplicar únicamente en Linux los modos POSIX `0700` a `.dbox` y `0600` a `data.db` y a los sidecars SQLite presentes `-wal`, `-shm` y `-journal`, sin reemplazar datos existentes.
- [x] 3.2 Convertir fallas al endurecer permisos en el error de base existente y preservar el comportamiento de inicialización sin prometer modos POSIX fuera de Linux.
- [x] 3.3 Añadir pruebas condicionadas a Linux para la creación y normalización de permisos, sidecars presentes y preservación de datos; cubrir que otros sistemas no exponen promesas de modos POSIX.

## 4. Validación integrada

- [x] 4.1 Verificar que la ayuda raíz expone `context` y que la salida JSON, stderr y exit codes existentes siguen cumpliendo sus contratos.
- [x] 4.2 Ejecutar `dotnet build Dbox.sln` y `dotnet test Dbox.sln`.
