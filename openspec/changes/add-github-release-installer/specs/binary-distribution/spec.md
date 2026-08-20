## Purpose

Permite instalar `dbox` desde GitHub Releases como un binario portable, sin clonar el repositorio, instalar el SDK de .NET ni usar privilegios de administrador.

## ADDED Requirements

### Requirement: Release binary coverage
Cada GitHub Release de `dbox` SHALL incluir binarios `self-contained` y `single-file` para `linux-x64`, `linux-arm64`, `win-x64`, `osx-x64` y `osx-arm64`. Los assets SHALL llamarse, respectivamente, `dbox-linux-x64`, `dbox-linux-arm64`, `dbox-win-x64.exe`, `dbox-osx-x64` y `dbox-osx-arm64`.

#### Scenario: Release publishes every supported asset
- **WHEN** se crea una release de `dbox`
- **THEN** sus assets contienen exactamente un ejecutable con el nombre esperado para cada una de las cinco plataformas soportadas

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
