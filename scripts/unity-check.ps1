# Opens the project in Unity batchmode, compiles, and reports compiler errors.
# Exit code: 0 = clean, 1 = compile errors, 2 = could not run. The Unity Editor must be closed.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe'
$log = Join-Path $root 'Logs\unity-check.log'
if (-not (Test-Path $unity)) { Write-Host "Unity not found: $unity"; exit 2 }
New-Item -ItemType Directory -Force (Split-Path $log) | Out-Null
if (Test-Path $log) { Remove-Item $log }

$p = Start-Process -FilePath $unity -Wait -PassThru -ArgumentList @(
    '-batchmode', '-quit', '-projectPath', "`"$root`"", '-logFile', "`"$log`"")
if (-not (Test-Path $log)) { Write-Host 'No log produced.'; exit 2 }

$errs = Select-String -Path $log -Pattern 'error CS\d+' | ForEach-Object { $_.Line.Trim() } | Sort-Object -Unique
$failed = Select-String -Path $log -Pattern 'Scripts have compiler errors|Compilation failed' -Quiet
if ($errs -or $failed) {
    Write-Host "COMPILE ERRORS ($(@($errs).Count)):"
    $errs | ForEach-Object { Write-Host "  $_" }
    exit 1
}
if ($p.ExitCode -ne 0) { Write-Host "Unity exited with code $($p.ExitCode); see $log"; exit 2 }
Write-Host 'Unity compile check: clean.'
exit 0
