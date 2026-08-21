## Context

No hay workflow de release, scripts de instalacion ni README en el repositorio. El proyecto .NET ya genera un ejecutable llamado `dbox` y su opcion integrada `--version` responde correctamente. Vease `proposal.md` para la motivacion y `specs/binary-distribution/spec.md` para el contrato observable.

## Goals / Non-Goals

**Goals:**
- Producir los cinco ejecutables nativos a partir de un unico proyecto .NET.
- Publicar los assets directamente en una GitHub Release al etiquetar una version.
- Instalar un unico ejecutable por usuario con scripts que no necesiten dependencias de .NET ni permisos administrativos.
- Mantener la descarga de la ultima release estable sin analizar JSON ni depender de `jq`.

**Non-Goals:**
- Paquetes de sistema, gestores de paquetes, autoactualizacion o firmas/verificacion de checksums.
- Soporte para Windows ARM64, Linux x86, musl/Alpine u otras combinaciones de RID no solicitadas.
- Modificar los contratos JSON, la base SQLite o los comandos funcionales de `dbox`.

## Decisions

### Publicar desde un workflow por tag

Un workflow de GitHub Actions se activara con tags de version y ejecutara una matriz de los cinco RIDs. Cada entrada ejecutara `dotnet publish` en configuracion `Release`, con `--self-contained true` y `-p:PublishSingleFile=true`, renombrara el archivo de salida al nombre de asset contractual y lo adjuntara a una unica Release.

Esto evita almacenar binarios en Git y permite que cada release sea reproducible desde el tag. La alternativa de publicar manualmente desde una maquina de desarrollo se descarta porque puede omitir plataformas o producir assets inconsistentes.

Un job de validacion ejecutara `dotnet test Dbox.sln` antes de la matriz de publicacion. Un job Windows ejecutara `install.ps1` con `%LOCALAPPDATA%` temporal y sustituira `Invoke-WebRequest` para comprobar su diagnostico de descarga sin efectuar otra instalacion. Despues de publicar los artefactos, smoke tests en macOS y Windows ejecutaran `dbox --version` sobre los binarios x64 nativos antes de crear la release. El token tendra `contents: read` por defecto y solo el job de release elevara a `contents: write`. La creacion de la release enumerara los cinco archivos esperados, en lugar de usar un glob que podria incorporar artefactos ajenos. Las actions se usaran en versiones con runtime Node.js mantenido y fijadas por SHA completo para que sus dependencias sean inmutables.

### Notas de release generadas

El job de release construira un bloque Markdown con la version, commit, enlace a la ejecucion, tabla de assets y comandos de instalacion y verificacion. `gh release create` recibira ese bloque mediante `--notes`, junto con `--generate-notes`, para anteponer la informacion operativa al changelog que genera GitHub. Tambien usara un titulo explicito `dbox <tag>` y `--verify-tag` para no crear tags implícitos.

### Descargar mediante la ruta `releases/latest/download`

Los instaladores descargaran `https://github.com/alexis-jose-calderon-diaz/dbox/releases/latest/download/<asset>`. GitHub resuelve esa ruta a la ultima release estable y redirige al asset exacto, sin exponer tokens ni requerir una herramienta para interpretar la API de Releases.

Consultar la API REST se descarta porque ampliaria las dependencias de shell y el manejo de JSON sin aportar funcionalidad necesaria para un instalador sencillo.

### Instalacion por usuario y verificacion en la sesion actual

`install.sh` instalara en `~/.local/bin`, creara el directorio y aplicara `chmod +x`. Si esa ruta no esta en `PATH`, mostrara una instruccion para agregarla; tambien la antepondra al `PATH` del proceso antes de ejecutar `dbox --version`, para verificar el binario recien descargado sin alterar archivos de perfil.

`install.ps1` instalara en `%LOCALAPPDATA%\dbox\bin`, agregara la ruta al `PATH` de usuario solo si falta y la antepondra al `PATH` del proceso para que `dbox --version` funcione en esa misma sesion. Escribir solo la variable de entorno de usuario evita privilegios administrativos.

Modificar automaticamente archivos de perfil Unix se descarta porque el shell y la configuracion de cada persona son ambiguos y esa modificacion no es necesaria para una instalacion segura.

### Detectar plataformas de forma estricta

El script Unix traducira `uname -s` y `uname -m` a los nombres de asset admitidos. PowerShell consultara la arquitectura del sistema operativo y solo aceptara x64. Cualquier valor fuera de la matriz falla antes de iniciar la descarga con un mensaje claro.

Ejecutar un asset de otra arquitectura mediante emulacion se descarta para mantener correspondencia directa entre plataforma detectada y asset publicado.

## Risks / Trade-offs

- [Una release no contiene un asset esperado o GitHub devuelve un error HTTP] → Las descargas usan opciones que convierten errores HTTP en fallos y los scripts imprimen un diagnostico que identifica el asset y la URL.
- [Una regresion llega a una release] → El workflow ejecuta la suite de pruebas antes de publicar y bloquea los jobs posteriores si falla.
- [Un binario de macOS o Windows no inicia] → Los runners nativos ejecutan una comprobacion `--version` antes de crear la release.
- [Un artefacto ajeno se adjunta a la release] → El comando de creacion enumera los cinco archivos permitidos de forma explicita.
- [Un ejecutable `dbox` anterior aparece antes en `PATH`] → Los scripts anteponen temporalmente su directorio de destino antes de verificar `dbox --version`.
- [El binario de Windows esta bloqueado por un proceso en ejecucion] → El instalador falla sin reemplazar parcialmente el ejecutable y comunica el error de copia.
- [No se publica Windows ARM64] → El instalador rechaza esa arquitectura de forma explicita hasta que exista un asset dedicado.
- [Los assets no incluyen verificacion criptografica adicional] → Se limita el alcance a la entrega directa de GitHub Releases; checksums o firmas se pueden incorporar en un cambio posterior.

## Migration Plan

1. Fusionar el workflow, los scripts y la documentacion.
2. Crear y publicar un tag de version para producir la primera GitHub Release con los cinco assets.
3. Probar cada instalador en las plataformas disponibles, incluyendo una descarga inexistente para validar el error HTTP.
4. Si un asset o instalador falla, eliminar o corregir la release y publicar una nueva version; no hay datos de usuario ni migraciones que revertir.
