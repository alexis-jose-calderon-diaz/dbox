# Instrucciones para OpenCode

## Fuentes de verdad

- Lee `PROJECT.md` antes de cambiar comportamiento, arquitectura, esquema, CLI, migraciones, pruebas o documentación; es el contrato funcional y tecnico del repositorio.
- `openspec/config.yaml` usa el workflow `spec-driven`. Los comandos y skills bajo `.opencode/` contienen las instrucciones operativas de OpenSpec y deben prevalecer sobre suposiciones del agente.
- El repositorio contiene la implementacion .NET y pruebas xUnit. Los comandos de validacion verificables son `dotnet build Dbox.sln` y `dotnet test Dbox.sln`; no inventes otros comandos sin agregarlos aqui.

## OpenSpec

- Los cambios viven en `openspec/changes/` y las especificaciones principales en `openspec/specs/`; usa las rutas devueltas por `openspec status --change "<name>" --json` en lugar de adivinarlas.
- Para iniciar un cambio usa `openspec new change "<name>"`, consulta `status` y luego las `instructions` del primer artefacto listo. `/opsx-new` avanza uno; `/opsx-continue` crea el siguiente; `/opsx-ff` crea todos los artefactos necesarios.
- Antes de implementar, `/opsx-apply` debe leer sus instrucciones y todos los `contextFiles`; si el cambio esta bloqueado o faltan artefactos, no implementes a ciegas.
- `/opsx-update` solo revisa artefactos de plan existentes y nunca codigo. Usa `/opsx-sync` para fusionar delta specs en las specs principales y `/opsx-verify` antes de `/opsx-archive`.
- Si el trabajo pertenece a un store registrado, ejecuta `openspec store list --json` y conserva `--store <id>` en los comandos que leen o escriben specs y changes.
- No fuerces al repositorio los archivos ignorados por `.opencode/.gitignore` (`node_modules`, `package.json` o `package-lock.json`) salvo solicitud explicita.

## Uso estricto de herramientas OpenCode

- Usa `todowrite` obligatoriamente antes de cualquier tarea con tres o mas pasos, varios archivos, commits o verificacion; actualiza el estado en tiempo real y manten exactamente una tarea `in_progress`.
- Marca una tarea como `completed` solo despues de ejecutar y revisar la verificacion correspondiente; no dejes tareas activas al terminar.
- Toda pregunta dirigida al usuario debe hacerse exclusivamente con la tool `question`; nunca escribas preguntas en mensajes normales. Agrupa las aclaraciones en una sola llamada y pregunta solo cuando el repositorio no permita resolver la ambiguedad.

## Implementacion

- Al agregar o modificar codigo, respeta el stack definido: .NET 10/C#, `System.CommandLine`, `Microsoft.EntityFrameworkCore.Sqlite`, migraciones EF Core y xUnit.
- No uses directamente `Microsoft.Data.Sqlite`, `SqliteConnection`, `SqliteCommand` ni SQL CRUD escrito a mano; tampoco agregues repositorios genericos, DI compleja o capas `Domain/Application/Infrastructure/Presentation`.
- La base es local por proyecto: `init` opera solo en el directorio actual y los demas comandos descubren el `.dbox/data.db` mas cercano. No agregues una base global ni una opcion `--database`.
- Las migraciones se generan con `dotnet-ef`, se aplican antes de las operaciones que usan la base y no se editan despues de aplicadas; un cambio de esquema requiere una migracion nueva.
- Las pruebas deben usar directorios y SQLite temporales independientes por caso, nunca rutas reales del usuario ni una base compartida; sigue la matriz minima de `PROJECT.md`.

## Git

- Manten mensajes de commit semanticos y concisos; el historial inicial usa prefijos convencionales como `docs:` y `chore:` y genera los mensages siempre en español.

## Formato de comandos de terminal

Cuando presentes comandos de terminal que contengan múltiples operaciones encadenadas con `&&`, escribe cada operación en una línea separada.

Mantén `&&` al final de la línea anterior para conservar explícitamente el encadenamiento entre comandos.

### Ejemplo correcto

```bash
dotnet restore &&
dotnet build &&
dotnet test
```

### Evitar

```bash
dotnet restore && dotnet build && dotnet test
```

Aplica esta regla tanto al mostrar comandos al usuario como al proponer comandos que puedan copiarse y ejecutarse directamente.
