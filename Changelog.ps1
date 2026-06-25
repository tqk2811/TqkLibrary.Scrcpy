# Regenerate CHANGELOG.md from Conventional Commits using git-cliff.
#
# Usage (run from repo root):
#   .\Changelog.ps1                 # full CHANGELOG.md (all tags + Unreleased)
#   .\Changelog.ps1 -Latest         # print only the latest released section
#   .\Changelog.ps1 -Unreleased     # print only commits since the last tag
#
# Prefers a locally installed 'git-cliff'; falls back to 'npx git-cliff'
# (requires Node.js). Install git-cliff for speed: winget install git-cliff

param(
    [switch]$Latest,
    [switch]$Unreleased,
    # Floor for the full changelog: the tag that OPENS the first minor line to show.
    # Sections are labelled by minor line, so 'v2.4.0' => changelog starts at the 2.4.x line.
    # Default 'v2.4.0' is the first published scrcpy line; pass '' to include the full history.
    [string]$Since = 'v2.4.0'
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

# Pick the git-cliff runner: native binary first, npx as fallback.
$cliffArgs = @('--config', "$PSScriptRoot\cliff.toml")

if ($Latest) {
    $cliffArgs += @('--latest')
}
elseif ($Unreleased) {
    $cliffArgs += @('--unreleased')
}
else {
    $cliffArgs += @('--output', "$PSScriptRoot\CHANGELOG.md")
    # Limit the full changelog to releases after $Since (positional commit range, must come last).
    if ($Since) { $cliffArgs += "$Since..HEAD" }
}

$native = Get-Command git-cliff -ErrorAction SilentlyContinue
if ($native) {
    Write-Host "Using git-cliff: $($native.Source)"
    & git-cliff @cliffArgs
}
else {
    $npx = Get-Command npx -ErrorAction SilentlyContinue
    if (-not $npx) {
        Write-Host "Neither 'git-cliff' nor 'npx' was found."
        Write-Host "Install one of:"
        Write-Host "  winget install git-cliff        (recommended, Windows)"
        Write-Host "  cargo install git-cliff"
        Write-Host "  npm  install -g git-cliff        (provides npx fallback)"
        exit 1
    }
    Write-Host "git-cliff not on PATH; falling back to 'npx -y git-cliff@latest'"
    & npx -y git-cliff@latest @cliffArgs
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "git-cliff failed (exit $LASTEXITCODE)."
    exit $LASTEXITCODE
}

if (-not ($Latest -or $Unreleased)) {
    Write-Host "CHANGELOG.md regenerated. Review, commit, then tag the release:"
    Write-Host "  git add CHANGELOG.md"
    Write-Host "  git commit -m 'docs: update changelog'"
}
