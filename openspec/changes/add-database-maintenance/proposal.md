## Why

Los proyectos locales necesitan una forma segura de preservar su base SQLite y
de diagnosticar su estado sin depender de herramientas externas ni alterar sus
datos. Las operaciones actuales migran automaticamente, por lo que no sirven
para una inspeccion estrictamente no invasiva.

## What Changes

- Agrega `dbox backup` como operacion raiz para crear una copia online
  consistente de la base de proyecto resuelta bajo `.dbox/backups`, con un
  nombre timestamp UTC y una respuesta JSON estable.
- Agrega `dbox doctor` como operacion raiz de diagnostico estrictamente
  read-only que informa existencia, apertura, integridad SQLite, migraciones
  pendientes y permisos sin migrar ni reparar.
- Actualiza el contrato de jerarquia y salida de la CLI para exponer ambos
  comandos raiz y sus respuestas JSON.
- Actualiza el contrato de base de proyecto para distinguir las operaciones de
  mantenimiento de los comandos que aplican migraciones automaticamente.
- Declara una excepcion tecnica minima y aislada para las operaciones de backup
  e integridad SQLite; no habilita SQLite directo ni SQL CRUD para el catalogo.

## Capabilities

### New Capabilities
- `database-maintenance`: Backup online consistente y diagnostico read-only de
  una base de proyecto SQLite resuelta.

### Modified Capabilities
- `cli-contract`: La raiz expone `backup` y `doctor` con sus contratos JSON.
- `project-database`: Las operaciones de mantenimiento resuelven la base sin
  aplicar migraciones y preservan el comportamiento de descubrimiento.

## Impact

Se modificaran los comandos raiz, la infraestructura de ubicacion y acceso a
SQLite, los modelos de salida y las pruebas de comandos e integracion. Antes de
implementar se actualizaran estos contratos fuente mediante los delta specs;
no se modificaran el catalogo `activity`, su CRUD ni sus reglas de acceso.
