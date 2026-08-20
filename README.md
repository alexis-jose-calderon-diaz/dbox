# dbox

`dbox` is a local project catalog database CLI backed by SQLite.

## Installation

### Linux/macOS

```bash
curl -fsSL https://raw.githubusercontent.com/alexis-jose-calderon-diaz/dbox/main/install.sh | bash
```

The installer places `dbox` in `~/.local/bin`. If that directory is not in your `PATH`, it prints the command required to add it for future shells.

### Windows

```powershell
irm https://raw.githubusercontent.com/alexis-jose-calderon-diaz/dbox/main/install.ps1 | iex
```

The installer places `dbox.exe` in `%LOCALAPPDATA%\dbox\bin` and adds that directory to the user `PATH` when needed. Windows ARM64 is not supported yet.

### Platform limitations

Linux releases target glibc-based x64 and ARM64 distributions. Alpine Linux and other musl-based distributions are not supported yet.

### Verify the installation

```bash
dbox --version
dbox --help
```
