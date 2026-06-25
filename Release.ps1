# Cut a release locally: regenerate CHANGELOG.md, stage it, and (optionally) make the release
# commit with the '[release]' marker that the CI workflow (release.yml) looks for, then push.
#
# Usage (from repo root):
#   .\Release.ps1                        # regenerate CHANGELOG.md + stage it, then commit yourself
#   .\Release.ps1 -Message "msg"         # also commit (marker '[release]' is appended if missing)
#   .\Release.ps1 -Message "msg" -Push   # also push -> triggers the CI release build
#
# Prerequisite: the tag 'vM.N.0' for the current minor line must already exist on origin
# (git tag vM.N.0; git push origin vM.N.0). CI builds M.N.<commits-since-tag>, attaches both
# nupkg (Scrcpy + Scrcpy.Wpf) to the GitHub Release 'vM.N.0' and pushes nuget.org.

param(
    [string]$Message,
    [switch]$Push
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

# 1) Regenerate CHANGELOG.md (full local timeline).
& "$PSScriptRoot\Changelog.ps1"
if ($LASTEXITCODE -ne 0) { throw "Changelog.ps1 failed (exit $LASTEXITCODE)" }

# 2) Stage the changelog.
git add CHANGELOG.md

# 3) Nothing staged at all -> nothing to release.
$staged = git diff --cached --name-only
if ([string]::IsNullOrWhiteSpace(($staged | Out-String))) {
    Write-Host ""
    Write-Host "Nothing to commit (CHANGELOG.md unchanged, no other staged changes)."
    Write-Host "To re-run CI without a new commit: gh workflow run release.yml --ref <branch>"
    return
}

# 4) No message -> leave it staged for the user to commit.
if (-not $Message) {
    Write-Host ""
    Write-Host "CHANGELOG.md regenerated and staged. To release, commit with the marker and push:"
    Write-Host "  git commit -m 'your message [release]'"
    Write-Host "  git push"
    return
}

# 5) Commit (ensure the [release] marker is present so CI builds), commits everything staged.
if ($Message -notmatch '\[release\]') { $Message = "$Message [release]" }

# Confirm before the release commit/push.
Write-Host ""
Write-Host "About to make the release commit:" -ForegroundColor Yellow
Write-Host "  message : $Message"
Write-Host "  staged  : $($staged -join ', ')"
if ($Push) { Write-Host "  then    : git push  (triggers the CI release build)" }
$null = Read-Host "Press Enter to proceed with the release (Ctrl+C to abort)"

git commit -m $Message
if ($LASTEXITCODE -ne 0) { throw "git commit failed (exit $LASTEXITCODE)" }

# 6) Push (triggers the CI release build).
if ($Push) {
    git push
    if ($LASTEXITCODE -ne 0) { throw "git push failed (exit $LASTEXITCODE)" }
    Write-Host "Pushed. CI release build should start (commit contains [release])."
} else {
    Write-Host "Committed. Run 'git push' to trigger the CI release build."
}
