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
- [Un ejecutable `dbox` anterior aparece antes en `PATH`] → Los scripts anteponen temporalmente su directorio de destino antes de verificar `dbox --version`.
- [El binario de Windows esta bloqueado por un proceso en ejecucion] → El instalador falla sin reemplazar parcialmente el ejecutable y comunica el error de copia.
- [No se publica Windows ARM64] → El instalador rechaza esa arquitectura de forma explicita hasta que exista un asset dedicado.
- [Los assets no incluyen verificacion criptografica adicional] → Se limita el alcance a la entrega directa de GitHub Releases; checksums o firmas se pueden incorporar en un cambio posterior.

## Migration Plan

1. Fusionar el workflow, los scripts y la documentacion.
2. Crear y publicar un tag de version para producir la primera GitHub Release con los cinco assets.
3. Probar cada instalador en las plataformas disponibles, incluyendo una descarga inexistente para validar el error HTTP.
4. Si un asset o instalador falla, eliminar o corregir la release y publicar una nueva version; no hay datos de usuario ni migraciones que revertir.
