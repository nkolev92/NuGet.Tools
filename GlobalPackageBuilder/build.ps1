<#
.SYNOPSIS
Builds NuGet client solutions and creates output artifacts.

.PARAMETER Configuration
Build configuration (debug by default)

.PARAMETER ReleaseLabel
Release label to use for package and assemblies versioning (zlocal by default)

.PARAMETER BuildNumber
Build number to use for package and assemblies versioning (auto-generated if not provided)

.PARAMETER MSPFXPath
Path to a code signing certificate for delay-sigining (optional)

.PARAMETER NuGetPFXPath
Path to a code signing certificate for delay-sigining (optional)

.PARAMETER SkipVS15
Skips building binaries targeting Visual Studio "15"

.PARAMETER Fast
Runs minimal incremental build. Skips end-to-end packaging step.

.PARAMETER CI
Indicates the build script is invoked from CI

.EXAMPLE
.\build.ps1
To run full clean build, e.g after switching branches

.EXAMPLE
.\build.ps1 -f
Fast incremental build

.EXAMPLE
.\build.ps1 -s15
To only run unit tests

.EXAMPLE
.\build.ps1 -v -ea Stop
To troubleshoot build issues
#>
[CmdletBinding()]
param (
    [string]$PackagesPath,
    [switch]$SkipClean
    )

if(-not $PackagesPath){
    $PackagesPath = ".\Packages"
}

if(-not $SkipClean){
    if (Test-Path $PackagesPath) {
        Write-Host 'Cleaning nupkgs folder'
        Remove-Item $PackagesPath\*.nupkg -Force
    }
    Write-Host "Running the project clean target"
    msbuild /t:clean
}


Write-Host "Building all packages"
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="ToolNet46X86" /p:ToolTargetFramework="net46" /p:ToolRuntimeIdentifier="win7-x86"
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="ToolNet46X64" /p:ToolTargetFramework="net46" /p:ToolRuntimeIdentifier="win7-x64"
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="ToolNet46Any" /p:ToolTargetFramework="net46" /p:ToolRuntimeIdentifier="any"
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="ToolAnyAny" /p:ToolTargetFramework="any" /p:ToolRuntimeIdentifier="any"
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="Toolnetcore20X64" /p:ToolTargetFramework="netcoreapp2.0" /p:ToolRuntimeIdentifier="win7-x64"