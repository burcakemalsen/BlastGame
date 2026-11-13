# Builds the WebGL player into Builds/WebGL in Unity batchmode. The Unity Editor must be closed.
# Exit code: 0 = built, non-zero = failure (see Logs/build-webgl.log).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe'
$log = Join-Path $root 'Logs\build-webgl.log'
$out = Join-Path $root 'Builds\WebGL'
New-Item -ItemType Directory -Force (Split-Path $log) | Out-Null

# Record which commit is being built, before Unity starts (deploy-webgl.ps1 reads this).
$commit = (git -C $root -c core.quotepath=false rev-parse HEAD)
if ($LASTEXITCODE -ne 0) { Write-Host 'git rev-parse HEAD failed.'; exit 1 }
$commit = $commit.Trim()
$short = (git -C $root -c core.quotepath=false rev-parse --short HEAD).Trim()
$dirty = git -C $root -c core.quotepath=false status --porcelain   # ignored files are not listed
if ($LASTEXITCODE -ne 0) { Write-Host 'git status failed.'; exit 1 }
$clean = -not ($dirty | Where-Object { $_ })
if (-not $clean) { Write-Host "Warning: working tree is not clean; this build cannot be deployed." }

if (Test-Path $out) { Remove-Item $out -Recurse -Force }

$p = Start-Process -FilePath $unity -Wait -PassThru -ArgumentList @(
    '-batchmode', '-quit', '-projectPath', "`"$root`"", '-buildTarget', 'WebGL',
    '-executeMethod', 'BuildScript.BuildWebGL', '-logFile', "`"$log`"")
if ($p.ExitCode -ne 0 -or -not (Test-Path (Join-Path $out 'index.html'))) {
    Write-Host "WebGL build FAILED (exit $($p.ExitCode)). Last errors:"
    Select-String -Path $log -Pattern 'error|Exception|FAILED' | Select-Object -Last 15 | ForEach-Object { Write-Host "  $($_.Line.Trim())" }
    if ($p.ExitCode -ne 0) { exit $p.ExitCode } else { exit 1 }
}
$info = [ordered]@{
    commit  = $commit
    short   = $short
    clean   = [bool]$clean
    builtAt = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
}
[System.IO.File]::WriteAllText((Join-Path $out 'build-info.json'), ($info | ConvertTo-Json), (New-Object System.Text.UTF8Encoding $false))
Write-Host "Build info: $short (clean=$clean)"
$size = (Get-ChildItem $out -Recurse -File | Measure-Object Length -Sum).Sum
Write-Host "WebGL build: $out  ($([math]::Round($size / 1MB, 2)) MB total)"
exit 0
