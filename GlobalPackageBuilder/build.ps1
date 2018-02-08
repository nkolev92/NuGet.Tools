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
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="ToolNet46X86" /p:TargetFramework="net461" /p:ToolRuntimeIdentifier="win7-x86" 
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="ToolNet46X64" /p:TargetFramework="net461" /p:ToolRuntimeIdentifier="win7-x64" 
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="ToolNet46Any" /p:TargetFramework="net461" /p:ToolRuntimeIdentifier="any" 
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="ToolAnyAny" /p:TargetFramework="net461" /p:ToolTargetFramework="any" /p:ToolRuntimeIdentifier="any"
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="Toolnetcore20X64" /p:TargetFramework="net461" /p:ToolTargetFramework="netcoreapp2.0" /p:ToolRuntimeIdentifier="win7-x64"
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="Toolnetcore20X86" /p:TargetFramework="net461" /p:ToolTargetFramework="netcoreapp2.0" /p:ToolRuntimeIdentifier="win7-x86"
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="Toolnetcore10X64" /p:TargetFramework="net461" /p:ToolTargetFramework="netcoreapp1.0" /p:ToolRuntimeIdentifier="win7-x64"
msbuild /t:build /restore /p:PackageOutputPath=$PackagesPath /p:PackageId="Toolnetcore10X86" /p:TargetFramework="net461" /p:ToolTargetFramework="netcoreapp1.0" /p:ToolRuntimeIdentifier="win7-x86"
