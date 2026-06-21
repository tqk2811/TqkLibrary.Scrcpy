$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
Set-Location $root

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) { throw "vswhere not found at $vswhere" }
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($msbuild)) { throw "MSBuild not found via vswhere (install C++ workload)" }
Write-Host "MSBuild: $msbuild"

if (-not (Get-Command dotnet-gitversion -ErrorAction SilentlyContinue)) {
    dotnet tool install -g GitVersion.Tool | Out-Host
    $env:PATH = "$env:PATH;$env:USERPROFILE\.dotnet\tools"
}
$gv = dotnet-gitversion /output json | ConvertFrom-Json
$verMajor = [int]$gv.Major; $verMinor = [int]$gv.Minor; $verBuild = [int]$gv.CommitsSinceVersionSource
Write-Host "Version: $verMajor.$verMinor.$verBuild"

$header = @'
#pragma once
#define VER_FILE_MAJOR    {0}
#define VER_FILE_MINOR    {1}
#define VER_FILE_BUILD    {2}
#define VER_FILE_REVISION 0
#define VER_FILEVERSION_STR    "{0}.{1}.{2}.0"
#define VER_PRODUCTVERSION_STR "{0}.{1}.{2}"
'@ -f $verMajor, $verMinor, $verBuild
[System.IO.File]::WriteAllText("$root\TqkLibrary.ScrcpyNative\version.generated.h", $header + "`r`n", (New-Object System.Text.UTF8Encoding($false)))

Remove-Item -Recurse -Force .\x64\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\x86\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\TqkLibrary.Scrcpy\bin\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\TqkLibrary.Scrcpy.Wpf\bin\Release\** -ErrorAction SilentlyContinue

# Restore native NuGet packages (vcxproj imports FFmpeg props/targets) before building
Write-Host "Restoring TqkLibrary.Scrcpy.sln ..."
nuget restore .\TqkLibrary.Scrcpy.sln | Out-Host
if ($LASTEXITCODE -ne 0) { throw "nuget restore failed" }

$nativeProj = "$root\TqkLibrary.ScrcpyNative\TqkLibrary.ScrcpyNative.vcxproj"
foreach ($platform in @('x64','Win32')) {
    Write-Host "Building native $platform ..."
    & $msbuild $nativeProj /t:Rebuild /p:Configuration=Release /p:Platform=$platform /p:SolutionDir="$root\" /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw "Native build failed ($platform)" }
}

dotnet pack .\TqkLibrary.Scrcpy\TqkLibrary.Scrcpy.csproj -c Release -o .\TqkLibrary.Scrcpy\bin\Release
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed (TqkLibrary.Scrcpy)" }
$nupkg = Get-ChildItem .\TqkLibrary.Scrcpy\bin\Release\*.nupkg | Select-Object -First 1
Write-Host "Packed: $($nupkg.Name)"

dotnet pack .\TqkLibrary.Scrcpy.Wpf\TqkLibrary.Scrcpy.Wpf.csproj -c Release -o .\TqkLibrary.Scrcpy.Wpf\bin\Release
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed (TqkLibrary.Scrcpy.Wpf)" }
$nupkgWpf = Get-ChildItem .\TqkLibrary.Scrcpy.Wpf\bin\Release\*.nupkg | Select-Object -First 1
Write-Host "Packed: $($nupkgWpf.Name)"

if (![string]::IsNullOrWhiteSpace($env:localNuget)) {
    Copy-Item $nupkg.FullName -Destination $env:localNuget -Force
    Copy-Item $nupkgWpf.FullName -Destination $env:localNuget -Force
}
if (![string]::IsNullOrWhiteSpace($env:nugetKey)) {
    Write-Host "Enter to push nuget"; pause; Write-Host "enter to confirm"; pause
    nuget push $nupkg.FullName -ApiKey $env:nugetKey -Source https://api.nuget.org/v3/index.json
    nuget push $nupkgWpf.FullName -ApiKey $env:nugetKey -Source https://api.nuget.org/v3/index.json
}
