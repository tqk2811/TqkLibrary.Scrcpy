Remove-Item -Recurse -Force .\x64\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\x86\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\TqkLibrary.Scrcpy\bin\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\TqkLibrary.Scrcpy.Wpf\bin\Release\** -ErrorAction SilentlyContinue

$env:PATH="$($env:PATH);C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE;C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE"
devenv .\TqkLibrary.Scrcpy.sln /Rebuild 'Release|x64' /Project TqkLibrary.ScrcpyNative
devenv .\TqkLibrary.Scrcpy.sln /Rebuild 'Release|x86' /Project TqkLibrary.ScrcpyNative

dotnet build --no-incremental .\TqkLibrary.Scrcpy\TqkLibrary.Scrcpy.csproj -c Release
nuget pack .\TqkLibrary.Scrcpy\TqkLibrary.Scrcpy.nuspec -Symbols -OutputDirectory .\TqkLibrary.Scrcpy\bin\Release

dotnet build --no-incremental .\TqkLibrary.Scrcpy.Wpf\TqkLibrary.Scrcpy.Wpf.csproj -c Release
nuget pack .\TqkLibrary.Scrcpy.Wpf\TqkLibrary.Scrcpy.Wpf.nuspec -Symbols -OutputDirectory .\TqkLibrary.Scrcpy.Wpf\bin\Release

$localNuget = $env:localNuget
if(![string]::IsNullOrWhiteSpace($localNuget))
{
    Copy-Item .\TqkLibrary.Scrcpy\bin\Release\*.nupkg -Destination $localNuget -Force
    Copy-Item .\TqkLibrary.Scrcpy.Wpf\bin\Release\*.nupkg -Destination $localNuget -Force
}

$nugetKey =$env:nugetKey
if(![string]::IsNullOrWhiteSpace($nugetKey))
{
    Write-Host "enter to push nuget"
    pause
    Write-Host "enter to confirm"
    pause

    $files = [System.IO.Directory]::GetFiles("$PSScriptRoot\TqkLibrary.Scrcpy\bin\Release","*.nupkg")
    iex "nuget push $($files[0]) -ApiKey $nugetKey -Source https://api.nuget.org/v3/index.json"

    $files = [System.IO.Directory]::GetFiles("$PSScriptRoot\TqkLibrary.Scrcpy.Wpf\bin\Release","*.nupkg")
    iex "nuget push $($files[0]) -ApiKey $nugetKey -Source https://api.nuget.org/v3/index.json"
}