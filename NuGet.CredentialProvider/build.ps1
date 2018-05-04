Write-Host Cleaning obj and bin directories
msbuild /t:clean /v:q 
Write-Host Cleaning the provider directory
rm -r artifacts

Write-Host Building assemblies
msbuild /restore /v:m

$netfxSourceDir = "CredentialProvider.Console\bin\Debug\net46\*"
$netfxDestDir = "artifacts/netfx/CredentialProvider.Console"
$netCoreDestDir = "../artifacts/netcore/CredentialProvider.Console"
mkdir $netfxDestDir
Copy-Item -Force -Recurse -Exclude *.pdb $netfxSourceDir -Destination $netfxDestDir

dotnet publish -r win-x64 --framework netcoreapp2.0 -o $netCoreDestDir

msbuild /t:restore /p:RestoreForce="true"

