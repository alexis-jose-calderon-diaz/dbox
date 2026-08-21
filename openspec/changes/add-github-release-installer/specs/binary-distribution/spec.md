## Purpose

Permite instalar `dbox` desde GitHub Releases como un binario portable, sin clonar el repositorio, instalar el SDK de .NET ni usar privilegios de administrador.

## ADDED Requirements

### Requirement: Release binary coverage
Cada GitHub Release de `dbox` SHALL incluir binarios `self-contained` y `single-file` para `linux-x64`, `linux-arm64`, `win-x64`, `osx-x64` y `osx-arm64`. Los assets SHALL llamarse, respectivamente, `dbox-linux-x64`, `dbox-linux-arm64`, `dbox-win-x64.exe`, `dbox-osx-x64` y `dbox-osx-arm64`.

#### Scenario: Release publishes every supported asset
- **WHEN** se crea una release de `dbox`
- **THEN** sus assets contienen exactamente un ejecutable con el nombre esperado para cada una de las cinco plataformas soportadas

### Requirement: Verified and least-privilege release workflow
El workflow de release SHALL ejecutar `dotnet test Dbox.sln` correctamente antes de publicar binarios, ejecutar `install.ps1` en un directorio `%LOCALAPPDATA%` temporal de un runner Windows x64 y comprobar su error de descarga HTTP, y ejecutar `dbox --version` con los binarios `osx-x64` y `win-x64` en runners de sus plataformas de destino. Los jobs de validacion y publicacion SHALL tener solo permiso de lectura de contenidos; unicamente el job que crea la GitHub Release tendra `contents: write`. El job de release SHALL adjuntar explicitamente los cinco assets contractuales y no cualquier artefacto disponible en la ejecucion.

#### Scenario: Failing tests prevent a release
- **WHEN** `dotnet test Dbox.sln` falla durante una ejecucion activada por un tag
- **THEN** no se publican binarios ni se crea una GitHub Release

#### Scenario: Release attaches only contract assets
- **WHEN** la ejecucion contiene un artefacto adicional ajeno a la distribucion de `dbox`
- **THEN** la GitHub Release solo adjunta los cinco ejecutables definidos por el contrato

#### Scenario: Cross-platform binaries start before release
- **WHEN** los assets para macOS x64 y Windows x64 se han publicado como artefactos del workflow
- **THEN** cada binario ejecuta `dbox --version` correctamente en un runner de su plataforma antes de crear la GitHub Release

#### Scenario: Windows installer validates download behavior
- **WHEN** una ejecucion activada por un tag valida `install.ps1` en un runner Windows x64
- **THEN** el instalador usa un directorio temporal, ejecuta `dbox --version` y su fallo HTTP contiene un mensaje de descarga entendible

### Requirement: Informative release notes
Cada GitHub Release de `dbox` SHALL tener un titulo `dbox <tag>` y notas que identifiquen la version, commit de origen, enlace a la ejecucion, plataformas y nombres de assets, comandos de instalacion y verificacion, y el changelog generado por GitHub.

#### Scenario: User consults a release
- **WHEN** una persona abre una GitHub Release de `dbox`
- **THEN** puede identificar que binarios contiene, como instalar la CLI, como verificarla y los cambios incluidos sin depender unicamente del nombre del tag

### Requirement: Unix installer selects and installs the platform binary
El repositorio SHALL incluir `install.sh`, ejecutable mediante una tuberia con `bash`, que detecte Linux o macOS y las arquitecturas x64 o ARM64, descargue el asset correspondiente de la ultima GitHub Release estable e instale `dbox` en `~/.local/bin/dbox`. El instalador SHALL crear el directorio si no existe, asignar permiso de ejecucion al archivo instalado y no requerir `sudo` ni el SDK de .NET.

#### Scenario: Supported Linux or macOS platform installs dbox
- **WHEN** una persona ejecuta `install.sh` en Linux o macOS con una combinacion soportada de sistema operativo y arquitectura
- **THEN** el binario correspondiente queda instalado como `~/.local/bin/dbox` y se verifica mediante `dbox --version`

#### Scenario: Unsupported Unix platform is rejected
- **WHEN** una persona ejecuta `install.sh` en un sistema operativo o arquitectura no soportados
- **THEN** el instalador termina sin instalar un binario y muestra un mensaje que identifica la plataforma no soportada

#### Scenario: Unix release download fails
- **WHEN** el asset de la ultima release estable no puede descargarse por un error HTTP
- **THEN** el instalador termina con error y muestra un mensaje entendible que indica que no pudo descargar el binario

### Requirement: Windows installer installs the x64 binary for the user
El repositorio SHALL incluir `install.ps1`, ejecutable mediante una tuberia con PowerShell, que acepte Windows x64, descargue `dbox-win-x64.exe` de la ultima GitHub Release estable e instale el binario en `%LOCALAPPDATA%\dbox\bin\dbox.exe`. El instalador SHALL agregar ese directorio al `PATH` de usuario cuando falte, verificar la instalacion con `dbox --version`, y no requerir privilegios de administrador ni el SDK de .NET.

#### Scenario: Windows x64 installs dbox and persists PATH
- **WHEN** una persona ejecuta `install.ps1` en Windows x64
- **THEN** `dbox.exe` queda instalado en `%LOCALAPPDATA%\dbox\bin`, esa carpeta esta presente una unica vez en el `PATH` de usuario y `dbox --version` se ejecuta correctamente

#### Scenario: Unsupported Windows architecture is rejected
- **WHEN** una persona ejecuta `install.ps1` en una arquitectura distinta de x64
- **THEN** el instalador termina sin instalar un binario y muestra un mensaje que identifica la arquitectura no soportada

#### Scenario: Windows release download fails
- **WHEN** `dbox-win-x64.exe` no puede descargarse por un error HTTP
- **THEN** el instalador termina con error y muestra un mensaje entendible que indica que no pudo descargar el binario

### Requirement: Installation documentation
El README SHALL documentar los comandos de instalacion por tuberia para Linux/macOS y Windows usando el repositorio oficial, y SHALL incluir los comandos `dbox --version` y `dbox --help` para verificar la disponibilidad de la CLI.

#### Scenario: User follows documented installation commands
- **WHEN** una persona consulta la seccion `Installation` del README
- **THEN** encuentra un comando para Linux/macOS, uno para Windows y los comandos de verificacion de la CLI
