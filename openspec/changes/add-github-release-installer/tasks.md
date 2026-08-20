## 1. Publicacion de binarios

- [x] 1.1 Configurar el proyecto para publicar ejecutables `self-contained` y `single-file` para los cinco RIDs soportados.
- [x] 1.2 Crear el workflow de GitHub Actions activado por tags que publique cada RID, renombre los ejecutables con los nombres de asset contractuales y cree la GitHub Release con los cinco assets.

## 2. Instaladores de usuario

- [x] 2.1 Crear `install.sh` con deteccion estricta de Linux/macOS y x64/ARM64, descarga con manejo de errores HTTP, instalacion en `~/.local/bin`, verificacion y aviso de `PATH`.
- [x] 2.2 Crear `install.ps1` con validacion de Windows x64, descarga con manejo de errores HTTP, instalacion en `%LOCALAPPDATA%\dbox\bin`, actualizacion idempotente de `PATH` de usuario y verificacion.

## 3. Documentacion y validacion

- [x] 3.1 Crear el README con los comandos de instalacion oficiales para Unix y Windows, y los comandos `dbox --version` y `dbox --help`.
- [x] 3.2 Generar los cinco binarios publicados y comprobar que sus nombres coinciden con los assets que solicitan los instaladores.
- [ ] 3.3 Probar la deteccion de plataforma y las rutas de error HTTP de ambos scripts sin modificar ubicaciones reales del usuario.
- [x] 3.4 Instalar en directorios de usuario temporales o aislados, ejecutar `dbox --version` y documentar las limitaciones de plataforma o validacion no disponible localmente.
- [x] 3.5 Ejecutar `dotnet build Dbox.sln` y `dotnet test Dbox.sln`.

## Validation Notes

- La instalacion Unix se valido en un directorio `HOME` temporal, incluyendo la ejecucion de `dbox --version`, una plataforma no soportada y una descarga HTTP 404.
- No hay PowerShell ni Windows disponibles en el entorno actual para ejecutar `install.ps1` y completar la validacion de su deteccion de arquitectura y error HTTP.
- `dotnet build Dbox.sln` finalizo correctamente. `dotnet test Dbox.sln` ejecuto 25 pruebas y fallo 17 por cambios concurrentes del contrato `activity` que no corresponden a este cambio de distribucion.
