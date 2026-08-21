## Context

La CLI ya centraliza el descubrimiento de `.dbox/data.db` y los comandos de
catalogo aplican migraciones antes de operar. Vease `proposal.md` y los delta
specs para el comportamiento nuevo. Backup y diagnostico requieren abrir una
base potencialmente desactualizada o danada sin convertir esa apertura en una
migracion ni en una operacion de catalogo.

## Goals / Non-Goals

**Goals:**

- Integrar `backup` y `doctor` como comandos raiz que reutilizan la ubicacion de
  proyecto resuelta.
- Crear backups consistentes aun cuando SQLite tenga actividad, sin copiar los
  archivos de journaling manualmente.
- Producir diagnosticos deterministas que no cambien la base, su historial de
  migraciones ni el sistema de archivos.
- Mantener el contrato JSON unico de la CLI y las pruebas aisladas por
  directorio temporal.

**Non-Goals:**

- Restaurar backups, limpiar backups, reparar integridad, aplicar migraciones o
  ofrecer diagnostico de datos del catalogo.
- Generalizar el uso de SQLite directo, SQL manual o repositorios fuera de esta
  infraestructura de mantenimiento.
- Cambiar el contrato de `activity` ni la semantica migratoria de sus comandos.

## Decisions

### Comandos raiz y respuestas

`DboxCli` registrara `backup` y `doctor` junto a `init`; sus handlers usaran
`DboxLocator` y `CommandExecutor` como los demas comandos. Las respuestas
exitosas seran objetos JSON estables:

```json
{ "database": ".dbox/data.db", "backup": ".dbox/backups/data-20260821T120000000Z.db" }
```

```json
{
  "database": ".dbox/data.db",
  "exists": true,
  "can_open": true,
  "integrity": "ok",
  "pending_migrations": [],
  "permissions": {
    "database_readable": true,
    "database_writable": true,
    "backup_directory_writable": true
  }
}
```

Las rutas son relativas a la raiz del proyecto resuelto, nunca al directorio
desde el que se invoca el comando. `integrity` usara `ok`, `failed` o
`not_checked`; si no se puede abrir la base, `can_open` sera `false`,
`integrity` sera `not_checked` y las migraciones pendientes se representaran
como `null`. Un resultado diagnosticable no saludable seguira siendo una
respuesta exitosa de `doctor`; los fallos que impidan construir el diagnostico
completo usaran el error de base existente.

Se eligen objetos dedicados de salida sobre textos humanos para preservar el
contrato automatizable. No se agrega `--output` ni una opcion de ruta de
backup, pues ambos contradicen el alcance local y la salida JSON existente.

### Backup SQLite online aislado

Un componente pequeno de infraestructura de mantenimiento abrira la base
origen y el archivo destino con SQLite y ejecutara `BackupDatabase`. El origen
se abrira sin permiso de escritura y el destino se creara bajo
`.dbox/backups`; el nombre sera `data-<UTC ISO basico con fracciones>.db`.
La operacion de copia es la unica que crea archivos.

Se acepta una referencia directa y localizada a `Microsoft.Data.Sqlite` para
esta API de backup y para las comprobaciones de integridad. No se usara para
leer ni modificar `activities` ni ninguna otra tabla de catalogo, y no se
introducira SQL CRUD manual. Copiar `data.db` mediante `File.Copy` se descarta:
no garantiza una instantanea consistente cuando hay WAL o escrituras activas.

### Doctor exclusivamente read-only

El diagnostico no llamara a `Database.MigrateAsync`. Abrira SQLite en modo
read-only para comprobar apertura y ejecutar `PRAGMA integrity_check`; este es
el unico SQL directo permitido y su resultado se convertira a los estados de
integridad del contrato. Consultara las migraciones pendientes mediante el
modelo de migraciones ya configurado, sin aplicar cambios. Las comprobaciones
de permisos inspeccionaran los metadatos de archivo y directorio disponibles,
sin pruebas de escritura ni creacion de directorios. Los tres campos de
permisos son indicadores informativos: representaran la capacidad observada o
inferida como `true` o `false`, y `null` cuando la plataforma o la ausencia del
directorio impida determinarla sin escribir.

Las excepciones de apertura e integridad se transformaran en campos del
diagnostico para que una base danada pueda inspeccionarse. No se implementan
acciones de recuperacion. Usar `MigrateAsync` antes de doctor se descarta porque
viola su garantia read-only; usar solo EF Core para la integridad se descarta
porque no expone el equivalente a `integrity_check`.

### Contratos fuente antes del codigo

La implementacion comenzara actualizando las specs principales con los delta
specs de este cambio. Solo despues se agregaran comandos, infraestructura y
pruebas. Esto mantiene `openspec/specs/` como contrato fuente antes de cambiar
el comportamiento ejecutable.

## Risks / Trade-offs

- [Una copia de archivo puede ser inconsistente con WAL o escrituras activas] ->
  Usar `BackupDatabase` en lugar de copiar el archivo SQLite directamente.
- [La API directa de SQLite puede extenderse indebidamente al catalogo] ->
  Limitarla a un componente de mantenimiento y cubrir que no ejecuta CRUD.
- [Una base corrupta puede impedir parte del diagnostico] -> Representar los
  chequeos no ejecutables como estados o `null`, sin ocultar la causa como una
  reparacion automatica.
- [Los permisos efectivos varian entre sistemas operativos] -> Informar los
  permisos inspeccionables por metadatos y no inferirlos mediante escrituras de
  prueba que violarian el modo read-only.
- [Dos backups simultaneos pueden competir por un nombre] -> Generar el nombre
  con precision fraccional UTC y crear el destino exclusivamente; una colision
  se tratara como error de base en vez de sobrescribir una copia existente.

## Migration Plan

1. Sincronizar primero los delta specs de `database-maintenance`, `cli-contract`
   y `project-database` con los contratos principales.
2. Implementar la infraestructura aislada, los comandos raiz y sus modelos de
   respuesta sin modificar migraciones ni tablas.
3. Agregar pruebas de comandos e integracion con directorios y bases SQLite
   temporales independientes, incluyendo pendientes, corrupcion y ausencia de
   migracion por `doctor`.
4. Validar compilacion y todas las pruebas antes de distribuir la CLI.

No hay migracion de esquema ni datos. Para revertir, retirar los comandos y el
componente de mantenimiento deja intactas las bases fuente; los archivos de
backup ya creados se conservan para no destruir datos del usuario.
