## Why

Usar `dbox` hoy requiere clonar el repositorio y disponer del SDK de .NET. Una distribucion de binarios autocontenidos mediante GitHub Releases permite instalar la CLI en las plataformas soportadas con un unico comando y sin privilegios de administrador.

## What Changes

- Publicar binarios `self-contained` y `single-file` para Linux x64/ARM64, macOS x64/ARM64 y Windows x64 al crear una release de GitHub.
- Agregar instaladores de usuario para Linux/macOS (`install.sh`) y Windows (`install.ps1`) que descarguen el artefacto apropiado de la ultima release estable.
- Instalar la CLI en una ruta de usuario, configurar o indicar la configuracion necesaria de `PATH`, y verificar la instalacion con `dbox --version`.
- Crear documentacion de instalacion y verificacion en el README.
- Validar la solucion antes de publicar, limitar el token de GitHub al job que crea la release y generar notas con instrucciones, assets, procedencia y changelog.
- Ejecutar en Windows una comprobacion aislada de `install.ps1` antes de publicar binarios.

## Capabilities

### New Capabilities
- `binary-distribution`: Publicacion de binarios portables de `dbox` e instalacion sin SDK ni permisos administrativos.

### Modified Capabilities

- Ninguna.

## Impact

- Configuracion de publicacion del proyecto `src/Dbox/Dbox.csproj`.
- Nuevo workflow de GitHub Actions para generar y adjuntar artefactos a Releases.
- Nuevos scripts de instalacion en la raiz del repositorio y nuevo `README.md`.
- No cambia los comandos de catalogo, el contrato JSON ni el esquema SQLite de `dbox`.
