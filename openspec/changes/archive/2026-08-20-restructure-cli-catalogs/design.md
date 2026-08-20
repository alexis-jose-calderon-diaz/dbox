## Context

La CLI actual registra `init`, `schema` y todas las operaciones de `activity` directamente en `DboxCli.BuildRootCommand`. La base, el localizador y las migraciones ya son compartidos por proyecto, por lo que no requieren una separación de almacenamiento para soportar nuevas areas. Vease `proposal.md` para la motivacion y los delta specs para el contrato observable.

## Goals / Non-Goals

**Goals:**

- Componer la CLI en una raiz de infraestructura y grupos de catalogo.
- Mantener `dbox init` como la unica operacion de inicializacion de la base compartida.
- Mover sin cambios funcionales del modelo las operaciones de actividades al grupo `activity`.
- Hacer que la ayuda y los errores de sintaxis reflejen exclusivamente la nueva API publica.
- Dejar una composicion que permita agregar catalogos futuros sin volver a aplanar la raiz.

**Non-Goals:**

- Agregar los grupos o tablas de `command` y `skill`.
- Modificar el esquema SQLite, el algoritmo de descubrimiento, los formatos de salida exitosos o los codigos de salida existentes.
- Mantener aliases o compatibilidad para las rutas planas retiradas.
- Introducir un framework de plugins, registro dinamico de catalogos o una capa de abstraccion adicional.

## Decisions

### La raiz solo contiene infraestructura compartida

`dbox init` permanece directamente en `RootCommand` porque crea la unica base local y ejecuta todas las migraciones de la version instalada. Los catalogos no tendran comandos `init` propios. La ayuda raiz incluira el grupo `activity`, pero no sus operaciones internas.

Alternativa considerada: mover `init` a `dbox activity init`. Se descarta porque su efecto no esta limitado a actividades y seria incorrecto cuando la base contenga catalogos adicionales.

### `activity` es un Command padre que concentra sus operaciones

Se creara un `Command("activity")` y se le agregaran `schema`, `add`, `list`, `get`, `update` y `delete`. El runtime, repositorio, validacion y base se conservaran; los handlers actuales se asociaran a los comandos hijos para minimizar cambios de comportamiento.

Alternativa considerada: crear un runtime separado por catalogo desde este cambio. Se descarta: solo existe un catalogo y esa abstraccion no es necesaria para componer un grupo de comandos.

### Las opciones compartidas conservan alcance recursivo

La opcion raiz `--output` seguira siendo recursiva para que las formas actuales de colocacion de salida funcionen con `dbox activity`. El comando `schema` conservara `--json` como forzado a JSON dentro de `activity`.

Alternativa considerada: duplicar `--output` en cada grupo. Se descarta por duplicar el contrato y el analisis de formato.

### El alias de schema se normaliza solo dentro de `activity`

La normalizacion transformara `dbox activity --schema [opciones]` en `dbox activity schema [opciones]`. No transformara `dbox --schema`; esa ruta llegara al parser y se reportara como sintaxis invalida. La deteccion de salida JSON para errores reconocera el alias solo cuando pertenezca al grupo `activity`.

Alternativa considerada: mantener `dbox --schema` como alias global. Se descarta porque expone un contrato particular de actividades desde una raiz reservada para infraestructura.

### La transición es una ruptura explícita

No se implementaran aliases para `dbox schema`, `add`, `list`, `get`, `update` ni `delete`. El parser debe rechazarlos antes de que resuelvan la base u operen datos. La documentacion se actualizara en la misma entrega para que la ruta de migracion sea inequívoca.

Alternativa considerada: aliases temporales. Se descarta por mantener dos contratos publicos y retrasar la separacion raiz-catalogo.

## Risks / Trade-offs

- [Los scripts existentes dejan de funcionar] → La ruptura se declara en proposal, specs, `PROJECT.md`, ayuda y pruebas; las rutas planas fallan de forma determinista con exit code de validacion.
- [La ayuda podria seguir revelando comandos planos por una composicion incompleta] → Se probaran las ayudas raiz y de `activity`, ademas de las rutas rechazadas.
- [El normalizador podria reescribir `--schema` fuera de contexto] → Se limitara al patron cuyo primer argumento de comando sea `activity` y se cubriran los aliases raiz rechazados.
- [Catalogos futuros podrian requerir operaciones de infraestructura adicionales] → La regla admite nuevos comandos raiz solo si afectan a todos los catalogos; las operaciones de datos permanecen encapsuladas por area.

## Migration Plan

1. Actualizar `PROJECT.md` para establecer la jerarquia y reemplazar todos los ejemplos de actividad.
2. Reorganizar la composicion de comandos y el alias de schema sin alterar base, entidades ni migraciones.
3. Migrar las pruebas de actividad a las rutas anidadas y agregar pruebas para ayuda y rechazo de rutas planas.
4. Ejecutar `dotnet build Dbox.sln` y `dotnet test Dbox.sln`.

No hay migracion de datos ni rollback compatible de CLI: volver a una version anterior restaura las rutas planas, mientras los datos existentes permanecen utilizables porque la base no cambia.
