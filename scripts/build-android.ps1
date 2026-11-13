# Builds Builds/Android/BlastGame.apk in Unity batchmode. The Unity Editor must be closed.
# Exit code: 0 = APK built, non-zero = failure (see Logs/build-android.log).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe'
$log = Join-Path $root 'Logs\build-android.log'
$apk = Join-Path $root 'Builds\Android\BlastGame.apk'
New-Item -ItemType Directory -Force (Split-Path $log) | Out-Null
# Gradle's prefab step fails when the user profile path has non-ASCII characters
# (found in Step 0: C:\Users\<name with a non-ASCII letter>\.gradle). Use an ASCII Gradle home inside the project.
if (-not $env:GRADLE_USER_HOME -and $env:USERPROFILE -match '[^\x00-\x7F]') {
    $env:GRADLE_USER_HOME = Join-Path $root 'Library\GradleHome'
    Write-Host "Non-ASCII user profile detected; GRADLE_USER_HOME = $env:GRADLE_USER_HOME"
}
if (Test-Path $apk) { Remove-Item $apk }

$p = Start-Process -FilePath $unity -Wait -PassThru -ArgumentList @(
    '-batchmode', '-quit', '-projectPath', "`"$root`"", '-buildTarget', 'Android',
    '-executeMethod', 'BuildScript.BuildAndroid', '-logFile', "`"$log`"")
if ($p.ExitCode -ne 0 -or -not (Test-Path $apk)) {
    Write-Host "Android build FAILED (exit $($p.ExitCode)). Last errors:"
    Select-String -Path $log -Pattern 'error|Exception|FAILED' | Select-Object -Last 15 | ForEach-Object { Write-Host "  $($_.Line.Trim())" }
    if ($p.ExitCode -ne 0) { exit $p.ExitCode } else { exit 1 }
}
$f = Get-Item $apk
Write-Host "APK: $($f.FullName)  ($([math]::Round($f.Length / 1MB, 2)) MB)"
exit 0
