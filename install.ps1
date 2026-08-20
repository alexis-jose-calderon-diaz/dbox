$ErrorActionPreference = "Stop"

$repository = "alexis-jose-calderon-diaz/dbox"
$architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture

if ($architecture -ne [System.Runtime.InteropServices.Architecture]::X64) {
    throw "Unsupported Windows architecture: $architecture. dbox supports Windows x64 only."
}

$installDirectory = Join-Path $env:LOCALAPPDATA "dbox\bin"
$installPath = Join-Path $installDirectory "dbox.exe"
$assetUrl = "https://github.com/$repository/releases/latest/download/dbox-win-x64.exe"
$temporaryPath = Join-Path ([System.IO.Path]::GetTempPath()) "dbox-$([System.Guid]::NewGuid()).exe"

try {
    Invoke-WebRequest -Uri $assetUrl -OutFile $temporaryPath -ErrorAction Stop
}
catch {
    throw "Failed to download dbox-win-x64.exe from the latest GitHub Release. $($_.Exception.Message)"
}

try {
    New-Item -ItemType Directory -Path $installDirectory -Force | Out-Null
    Move-Item -Path $temporaryPath -Destination $installPath -Force
}
finally {
    Remove-Item -Path $temporaryPath -Force -ErrorAction SilentlyContinue
}

$userPath = [Environment]::GetEnvironmentVariable("Path", "User")
$normalizedInstallDirectory = $installDirectory.TrimEnd([char]'\')
$pathEntries = @($userPath -split ";" | Where-Object {
    $_ -and -not [string]::Equals(
        $_.TrimEnd([char]'\'),
        $normalizedInstallDirectory,
        [System.StringComparison]::OrdinalIgnoreCase)
})

$newUserPath = @($pathEntries + $installDirectory) -join ";"
if ($newUserPath -ne $userPath) {
    [Environment]::SetEnvironmentVariable("Path", $newUserPath, "User")
}

$env:Path = "$installDirectory;$env:Path"
Write-Host "Installed dbox to $installPath"
dbox --version
