<#
.SYNOPSIS
Builds a bunch of global tools packages.

.PARAMETER PackagesPath
Path where the packages will be output

.PARAMETER SkipClean
Whether to Skip Clean

.EXAMPLE
.\build.ps1 -PackagesPath .\Packages
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
msbuild /t:pack /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="TestPackage.Deprecation.Legacy"  
msbuild /t:pack /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="TestPackage.Deprecation.CriticalBugs"  
msbuild /t:pack /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="TestPackage.Deprecation.Other"  
msbuild /t:pack /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="TestPackage.Deprecation.Legacy.AlternatePackage"  
msbuild /t:pack /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="TestPackage.Deprecation.CriticalBugs.AlternatePackage"  
msbuild /t:pack /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="TestPackage.Deprecation.Other.AlternatePackage"  