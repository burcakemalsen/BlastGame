# Pre-push scan (docs/RULES.md, "Push oncesi tarama"). Run from anywhere inside the repo.
# Modes (pick one; default is -Staged):
#   (none)          staged changes only (use before a commit)
#   -All            every file tracked in HEAD, every line (use before the first push)
#   -Range <range>  added lines and file names in a rev range, e.g. origin/main..HEAD (use before later pushes)
# Checks: 1) .gitignore covers secret files, 2) key-like text,
#         3) names from the local term list (kept inside .git/info, never committed; whole word,
#            case-sensitive) and .pdf/.zip/.docx files.
# Exit code: 0 = clean, 1 = findings, 2 = scan could not run. Found terms are never printed.
param(
    [switch]$All,
    [string]$Range
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
if ($All -and $Range) { Write-Host 'Use either -All or -Range, not both.'; exit 2 }
$script:bad = 0

# --- 1. .gitignore covers secrets
$probe = @('.env', 'release.keystore', 'my.jks', 'secrets.json', 'api_token.txt')
foreach ($f in $probe) {
    git check-ignore -q -- $f
    if ($LASTEXITCODE -ne 0) { Write-Host "[1] .gitignore does NOT cover: $f"; $script:bad = 1 }
}

# --- load scan terms: <git common dir>/info/scan-terms, one term per line, '#' comments and blank lines skipped
$gitCommon = (git rev-parse --git-common-dir).Trim()
$termFile = Join-Path (Join-Path $root $gitCommon) 'info\scan-terms'
if ([System.IO.Path]::IsPathRooted($gitCommon)) { $termFile = Join-Path $gitCommon 'info\scan-terms' }
if (-not (Test-Path $termFile)) { Write-Host 'ERROR: local term list (.git/info/scan-terms) missing; cannot scan names.'; exit 2 }
$terms = @(Get-Content $termFile -Encoding UTF8 | ForEach-Object { $_.Trim() } | Where-Object { $_ -and -not $_.StartsWith('#') })
if ($terms.Count -eq 0) { Write-Host 'ERROR: local term list is empty.'; exit 2 }
$termRx = $terms | ForEach-Object { '(?<![\p{L}\p{N}_])' + [regex]::Escape($_) + '(?![\p{L}\p{N}_])' }

$keyPatterns = @(
    'AKIA[0-9A-Z]{16}', '-----BEGIN [A-Z ]*PRIVATE KEY-----', 'ghp_[A-Za-z0-9]{30,}',
    'github_pat_[A-Za-z0-9_]{30,}', 'sk-[A-Za-z0-9_-]{20,}', 'AIza[0-9A-Za-z_-]{30,}',
    '(?i)(api[_-]?key|secret|token|passw(or)?d|storepass|keypass)\s*[:=]\s*["'']?[A-Za-z0-9+/_\-]{12,}'
)

function Test-Name($name) {
    if ($name -match '\.(pdf|zip|docx)$') { Write-Host "[3] forbidden extension: $name"; $script:bad = 1 }
    foreach ($rx in $termRx) {
        if ($name -cmatch $rx) { Write-Host "[3] scan term in file name: $name"; $script:bad = 1; break }
    }
}

function Test-Line($file, $ln, $text) {
    foreach ($pat in $keyPatterns) {
        if ($text -match $pat) { Write-Host "[2] key-like text: ${file}:$ln"; $script:bad = 1; break }
    }
    foreach ($rx in $termRx) {
        if ($text -cmatch $rx) { Write-Host "[3] scan term found: ${file}:$ln"; $script:bad = 1; break }
    }
}

# Reads added lines from a unified diff (git diff -U0 ...) and tests them.
function Test-Diff($diffArgs) {
    $file = $null; $ln = 0
    foreach ($l in (git diff @diffArgs -U0 --no-color)) {
        if ($l -match '^\+\+\+ b/(.*)$') { $file = $Matches[1]; continue }
        if ($l -match '^@@ .*\+(\d+)') { $ln = [int]$Matches[1] - 1; continue }
        if ($l.StartsWith('+') -and -not $l.StartsWith('+++')) { $ln++; Test-Line $file $ln $l.Substring(1) }
    }
}

if ($All) {
    $mode = 'all tracked files in HEAD'
    $files = @(git ls-tree -r --name-only HEAD)
    foreach ($f in $files) {
        Test-Name $f
        $bytes = [System.IO.File]::ReadAllBytes((Join-Path $root $f))
        if ($bytes.Length -eq 0) { continue }
        $head = $bytes[0..([Math]::Min($bytes.Length, 8000) - 1)]
        if ($head -contains 0) { continue }   # binary: name checked only
        $content = git show "HEAD:$f"
        $ln = 0
        foreach ($t in $content) { $ln++; Test-Line $f $ln $t }
    }
} elseif ($Range) {
    $mode = "range $Range"
    foreach ($f in @(git diff --name-only --diff-filter=ACMR $Range)) { Test-Name $f }
    Test-Diff @($Range)
} else {
    $mode = 'staged changes'
    foreach ($f in @(git diff --staged --name-only --diff-filter=ACMR)) { Test-Name $f }
    Test-Diff @('--staged')
}

if ($script:bad) { Write-Host "PRE-PUSH SCAN ($mode): FINDINGS. Do not push."; exit 1 }
Write-Host "PRE-PUSH SCAN ($mode): clean."
exit 0
