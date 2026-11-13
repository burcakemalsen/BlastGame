# Commits Builds/WebGL into the local gh-pages branch using a separate git worktree.
# Never touches the main working tree, never pushes, never forces.
# First run creates an orphan gh-pages branch; later runs add normal commits.
# Exit code: 0 = committed (or nothing changed), non-zero = failure.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
$src = Join-Path $root 'Builds\WebGL'
if (-not (Test-Path (Join-Path $src 'index.html'))) { Write-Host "No WebGL build at $src. Run scripts/build-webgl.ps1 first."; exit 1 }
$short = (git rev-parse --short HEAD).Trim()
$wt = Join-Path $root 'Builds\gh-pages-worktree'   # inside ignored Builds/

git worktree prune
if (Test-Path $wt) { git worktree remove --force $wt }

git show-ref --verify --quiet refs/heads/gh-pages
$exists = ($LASTEXITCODE -eq 0)
if ($exists) {
    git worktree add $wt gh-pages
} else {
    git worktree add --orphan -b gh-pages $wt
}
if ($LASTEXITCODE -ne 0) { Write-Host 'git worktree add failed.'; exit 1 }

try {
    # replace the contents (except .git) with the build
    Get-ChildItem $wt -Force | Where-Object { $_.Name -ne '.git' } | Remove-Item -Recurse -Force
    Copy-Item -Path (Join-Path $src '*') -Destination $wt -Recurse -Force
    New-Item -ItemType File -Force (Join-Path $wt '.nojekyll') | Out-Null

    git -C $wt -c core.autocrlf=false add -A
    git -C $wt diff --staged --quiet
    if ($LASTEXITCODE -eq 0) { Write-Host 'gh-pages: nothing changed.' }
    else {
        git -C $wt -c core.autocrlf=false commit -q -m "deploy: webgl build $short" -m 'Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>'
        if ($LASTEXITCODE -ne 0) { Write-Host 'commit failed.'; exit 1 }
        Write-Host (git log gh-pages --oneline -1)
    }
}
finally {
    Set-Location $root
    git worktree remove --force $wt
}
Write-Host 'To publish: git push origin gh-pages'
exit 0
