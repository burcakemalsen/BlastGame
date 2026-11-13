# Pre-push scan (docs/RULES.md, "Push oncesi tarama"). Run from anywhere inside the repo.
# Modes (pick one; default is -Staged):
#   (none)          staged changes only (use before a commit)
#   -All            every file tracked in HEAD, every line, plus every commit message reachable from HEAD
#                   (use before the first push)
#   -Range <range>  for every commit in a rev range (e.g. origin/main..HEAD): its added lines, file names
#                   and commit message (use before later pushes)
# Checks: 1) .gitignore covers secret files, 2) key-like text,
#         3) names from the local term list (kept inside .git/info, never committed; whole word,
#            case-insensitive by default, a 'cs:' prefix makes a term case-sensitive) and .pdf/.zip/.docx files.
# Exit code: 0 = clean, 1 = findings, 2 = scan could not run. Found terms are never printed.
param(
    [switch]$All,
    [string]$Range
)
$ErrorActionPreference = 'Stop'
trap { Write-Host "ERROR: scan could not run: $($_.Exception.Message)"; exit 2 }

# Windows PowerShell 5.1 decodes native output with the OEM code page (e.g. ibm857), which breaks
# UTF-8 text such as Turkish letters. Read and write git's output as UTF-8.
$utf8 = New-Object System.Text.UTF8Encoding $false
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

# git with unquoted UTF-8 file names; stops the scan (exit 2) if git fails.
function G {
    $out = & git -c core.quotepath=false -c i18n.logOutputEncoding=UTF-8 @args
    if ($LASTEXITCODE -ne 0) { Write-Host "ERROR: git $($args[0]) failed (exit $LASTEXITCODE)."; exit 2 }
    $out
}

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

# --- load scan terms: <git common dir>/info/scan-terms, one term per line, '#' comments and blank lines skipped.
#     Default: case-insensitive (culture-invariant). 'cs:<term>': case-sensitive.
$gitCommon = (G rev-parse --git-common-dir).Trim()
$termFile = Join-Path (Join-Path $root $gitCommon) 'info\scan-terms'
if ([System.IO.Path]::IsPathRooted($gitCommon)) { $termFile = Join-Path $gitCommon 'info\scan-terms' }
if (-not (Test-Path $termFile)) { Write-Host 'ERROR: local term list (.git/info/scan-terms) missing; cannot scan names.'; exit 2 }
$termRx = @()
foreach ($line in (Get-Content $termFile -Encoding UTF8)) {
    $t = $line.Trim()
    if (-not $t -or $t.StartsWith('#')) { continue }
    $opt = [System.Text.RegularExpressions.RegexOptions]::CultureInvariant
    if ($t.StartsWith('cs:')) { $t = $t.Substring(3).Trim() }
    else { $opt = $opt -bor [System.Text.RegularExpressions.RegexOptions]::IgnoreCase }
    if (-not $t) { continue }
    $termRx += New-Object System.Text.RegularExpressions.Regex ('(?<![\p{L}\p{N}_])' + [regex]::Escape($t) + '(?![\p{L}\p{N}_])'), $opt
}
if ($termRx.Count -eq 0) { Write-Host 'ERROR: local term list is empty.'; exit 2 }

$keyPatterns = @(
    'AKIA[0-9A-Z]{16}', '-----BEGIN [A-Z ]*PRIVATE KEY-----', 'ghp_[A-Za-z0-9]{30,}',
    'github_pat_[A-Za-z0-9_]{30,}', 'sk-[A-Za-z0-9_-]{20,}', 'AIza[0-9A-Za-z_-]{30,}',
    '(?i)(api[_-]?key|secret|token|passw(or)?d|storepass|keypass)\s*[:=]\s*["'']?[A-Za-z0-9+/_\-]{12,}'
)

function Test-Term($text) {
    foreach ($rx in $termRx) { if ($rx.IsMatch($text)) { return $true } }
    return $false
}

# Masks any term inside a text before it is printed (file names can contain terms).
function Hide($text) {
    foreach ($rx in $termRx) { $text = $rx.Replace($text, '***') }
    $text
}

function Test-Name($name) {
    if ($name -match '\.(pdf|zip|docx)$') { Write-Host "[3] forbidden extension: $(Hide $name)"; $script:bad = 1 }
    if (Test-Term $name) { Write-Host "[3] scan term in file name: $(Hide $name)"; $script:bad = 1 }
}

function Test-Line($file, $ln, $text) {
    foreach ($pat in $keyPatterns) {
        if ($text -match $pat) { Write-Host "[2] key-like text: $(Hide $file):$ln"; $script:bad = 1; break }
    }
    if (Test-Term $text) { Write-Host "[3] scan term found: $(Hide $file):$ln"; $script:bad = 1 }
}

# Reads added lines from a unified diff (-U0) and tests them; $prefix marks where the diff came from.
function Test-DiffLines($lines, $prefix) {
    $file = $null; $ln = 0
    foreach ($l in $lines) {
        if ($l -match '^\+\+\+ b/(.*)$') { $file = $prefix + $Matches[1]; continue }
        if ($l -match '^@@ .*\+(\d+)') { $ln = [int]$Matches[1] - 1; continue }
        if ($l.StartsWith('+') -and -not $l.StartsWith('+++')) { $ln++; Test-Line $file $ln $l.Substring(1) }
    }
}

# Tests every line of a commit message.
function Test-Message($c) {
    $short = $c.Substring(0, 7)
    $ln = 0
    foreach ($t in (G show -s --format=%B $c)) { $ln++; Test-Line "commit $short message" $ln $t }
}

if ($All) {
    $mode = 'all tracked files in HEAD and all commit messages'
    $files = @(G ls-tree -r --name-only HEAD)
    foreach ($f in $files) {
        Test-Name $f
        $bytes = [System.IO.File]::ReadAllBytes((Join-Path $root $f))
        if ($bytes.Length -eq 0) { continue }
        $head = $bytes[0..([Math]::Min($bytes.Length, 8000) - 1)]
        if ($head -contains 0) { continue }   # binary: name checked only
        $content = G show "HEAD:$f"
        $ln = 0
        foreach ($t in $content) { $ln++; Test-Line $f $ln $t }
    }
    foreach ($c in @(G rev-list HEAD)) { Test-Message $c }
} elseif ($Range) {
    $mode = "range $Range, every commit"
    $commits = @(G rev-list $Range)
    foreach ($c in $commits) {
        $short = $c.Substring(0, 7)
        foreach ($f in @(G show --format= --name-only --diff-filter=ACMR --diff-merges=first-parent $c)) {
            if ($f) { Test-Name $f }
        }
        Test-DiffLines (G show --format= -U0 --no-color --no-ext-diff --diff-merges=first-parent $c) "commit ${short}: "
        Test-Message $c
    }
} else {
    $mode = 'staged changes'
    foreach ($f in @(G diff --staged --name-only --diff-filter=ACMR)) { Test-Name $f }
    Test-DiffLines (G diff --staged -U0 --no-color --no-ext-diff) ''
}

if ($script:bad) { Write-Host "PRE-PUSH SCAN ($mode): FINDINGS. Do not push."; exit 1 }
Write-Host "PRE-PUSH SCAN ($mode): clean."
exit 0
