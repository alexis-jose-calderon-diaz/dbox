## 1. Contratos fuente

- [ ] 1.1 Sincronizar los delta specs `database-maintenance`, `cli-contract` y `project-database` con las especificaciones principales antes de editar la implementacion.
- [ ] 1.2 Verificar que los contratos sincronizados preserven la migracion automatica de los comandos de catalogo y excluyan de ella a `backup` y `doctor`.

## 2. Infraestructura de mantenimiento

- [ ] 2.1 Agregar la dependencia directa minima de SQLite y un componente aislado para copiar una base con `BackupDatabase`, sin SQL CRUD del catalogo.
- [ ] 2.2 Implementar la generacion exclusiva de rutas `.dbox/backups/data-<timestamp UTC>.db` y la respuesta JSON de backup relativa a la raiz de proyecto resuelta.
- [ ] 2.3 Implementar el diagnostico read-only de apertura, `integrity_check`, migraciones pendientes y permisos inspeccionables, sin `MigrateAsync`, reparacion ni escritura.
- [ ] 2.4 Modelar las respuestas de backup y doctor, incluidos estados de integridad no ejecutable y permisos indeterminables, dentro del contrato JSON existente.

## 3. Comandos raiz

- [ ] 3.1 Registrar `dbox backup` y `dbox doctor` en la raiz de la CLI con ayuda nativa y descubrimiento mediante `DboxLocator`.
- [ ] 3.2 Conectar `backup` a la infraestructura consistente y mapear ausencia de base o fallos de copia a los errores y exit codes existentes.
- [ ] 3.3 Conectar `doctor` al diagnostico read-only, devolviendo resultados no saludables como JSON sin migrar ni modificar archivos.

## 4. Pruebas

- [ ] 4.1 Agregar pruebas de `backup` en directorios temporales para la ruta por defecto, nombre UTC, contenido consistente, descubrimiento desde descendientes y ausencia de base.
- [ ] 4.2 Agregar pruebas de `doctor` para una base sana, migraciones pendientes sin cambios en el historial, permisos informativos y ausencia de base.
- [ ] 4.3 Agregar pruebas de `doctor` para fallo de apertura o integridad que confirmen salida diagnostica, ausencia de reparacion y ausencia de migracion.
- [ ] 4.4 Cubrir que los comandos raiz aparecen en la ayuda, emiten exactamente un JSON y no habilitan acceso SQLite directo ni CRUD en `activity`.

## 5. Validacion

- [ ] 5.1 Ejecutar `dotnet build Dbox.sln`.
- [ ] 5.2 Ejecutar `dotnet test Dbox.sln`.
- [ ] 5.3 Ejecutar `openspec validate add-database-maintenance --strict` y corregir cualquier incumplimiento de los artefactos.
