$dirInfo= New-Object -Typename System.IO.DirectoryInfo -ArgumentList ($PSScriptRoot)
Set-Location $PSScriptRoot

$id="TqkLibrary.ScrcpyNative-x64"
$buildDay=[DateTime]::Now.ToString("yyyyMMdd")
iex "nuget pack .\TqkLibrary.ScrcpyNative-ReleaseNoScreenShot.nuspec -OutputDirectory .\x64 -p 'id=$($id);build=$($buildDay)'"