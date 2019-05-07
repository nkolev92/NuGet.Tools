Import-Module Posh-Git
Import-Module PSReadLine  
Set-PSReadlineKeyHandler -Key Tab -Function Complete
Set-PSReadlineOption -BellStyle None
Set-Location F:\NuGet.Client
$nugetClientRoot = "F:\NuGet.Client"
$cliRoot = "F:\Forks\cli"

Function Invoke-NuGetTargetsCustom()
{
	$restoreDllPath = Join-Path $nugetClientRoot "artifacts\NuGet.Build.Tasks\16.0\bin\Debug\net472\NuGet.Build.Tasks.dll"
	$nugetRestoreTargetsPath = Join-Path $nugetClientRoot "src\NuGet.Core\NuGet.Build.Tasks\NuGet.targets"
	Write-Host "msbuild /p:NuGetRestoreTargets=$nugetRestoreTargetsPath /p:RestoreTaskAssemblyFile=$restoreDllPath $($args[0..$args.Count])" 
	& msbuild /p:NuGetRestoreTargets=$nugetRestoreTargetsPath /p:RestoreTaskAssemblyFile=$restoreDllPath $args[0..$args.Count]
} 

Function Invoke-PackTargetsCustom()
{
	$packDllPath = Join-Path $nugetClientRoot "artifacts\NuGet.Build.Tasks.Pack\16.0\bin\Debug\net472\NuGet.Build.Tasks.Pack.dll"
	$packTargetsPath = Join-Path $nugetClientRoot "src\NuGet.Core\NuGet.Build.Tasks.Pack\NuGet.Build.Tasks.Pack.targets"
	Write-Host "msbuild /p:NuGetBuildTasksPackTargets=$packTargetsPath /p:ImportNuGetBuildTasksPackTargetsFromSdk="true" /p:NuGetPackTaskAssemblyFile=$packDllPath $($args[0..$args.Count])" 
	& msbuild /p:NuGetBuildTasksPackTargets=$packTargetsPath /p:ImportNuGetBuildTasksPackTargetsFromSdk="true" /p:NuGetPackTaskAssemblyFile=$packDllPath $args[0..$args.Count]
}

Function Invoke-NuGetCustom()
{
	$packDllPath = Join-Path $nugetClientRoot "artifacts\NuGet.Build.Tasks.Pack\16.0\bin\Debug\net472\NuGet.Build.Tasks.Pack.dll"
	$packTargetsPath = Join-Path $nugetClientRoot "src\NuGet.Core\NuGet.Build.Tasks.Pack\NuGet.Build.Tasks.Pack.targets"
	
	$restoreDllPath = Join-Path $nugetClientRoot "artifacts\NuGet.Build.Tasks\16.0\bin\Debug\net472\NuGet.Build.Tasks.dll"
	$nugetRestoreTargetsPath = Join-Path $nugetClientRoot "src\NuGet.Core\NuGet.Build.Tasks\NuGet.targets"

	Write-Host "msbuild /p:NuGetRestoreTargets=$nugetRestoreTargetsPath /p:RestoreTaskAssemblyFile=$restoreDllPath /p:NuGetBuildTasksPackTargets=$packTargetsPath /p:ImportNuGetBuildTasksPackTargetsFromSdk="true" /p:NuGetPackTaskAssemblyFile=$packDllPath $($args[0..$args.Count])" 
	& msbuild /p:NuGetRestoreTargets=$nugetRestoreTargetsPath /p:RestoreTaskAssemblyFile=$restoreDllPath /p:NuGetBuildTasksPackTargets=$packTargetsPath /p:ImportNuGetBuildTasksPackTargetsFromSdk="true" /p:NuGetPackTaskAssemblyFile=$packDllPath $args[0..$args.Count]
}

Function Run-NuGetTargetsCustom() {
    $projectPath = $args[0]
    $target = $args[1]

    $publishDllPath = Join-Path $nugetClientRoot "artifacts\NuGet.Build.Tasks\16.0\bin\Debug\net472\NuGet.Build.Tasks.dll"
    $targetsPath = Join-Path $nugetClientRoot "src\NuGet.Core\NuGet.Build.Tasks\NuGet.targets"
    
    Write-Host "msbuild $projectPath /t:$target /p:NuGetRestoreTargets=$targetsPath /p:RestoreTaskAssemblyFile=$publishDllPath $($args[2..$args.Count])" -foregroundcolor "cyan"
    & msbuild $projectPath /t:$target /p:NuGetRestoreTargets=$targetsPath /p:RestoreTaskAssemblyFile=$publishDllPath $args[2..$args.Count]
} 

Function Remove-OrphanedLocalBranches() {
    @(git branch -vv) | findstr ": gone]" | findstr /V "\*" | %{$_.Split(' ')[2];} | findstr /V "^release" | % { git branch -D $_}
}

Function Run-NuGetTargetsReleaseCustom($projectPath, $target, $extra) {
    $publishDllPath = Join-Path $nugetClientRoot "artifacts\NuGet.Build.Tasks\16.0\bin\Release\net46\NuGet.Build.Tasks.dll"
    $targetsPath = Join-Path $nugetClientRoot "src\NuGet.Core\NuGet.Build.Tasks\NuGet.targets"
    
    Write-Host "msbuild $projectPath /t:$target /p:NuGetRestoreTargets=$targetsPath /p:RestoreTaskAssemblyFile=$publishDllPath $extra"    
    & msbuild $projectPath /t:$target /p:NuGetRestoreTargets=$targetsPath /p:RestoreTaskAssemblyFile=$publishDllPath $extra
} 

Function Run-TestsWithFilter {    
    <#
  .SYNOPSIS
  Restores, Builds and runs tests.
  .DESCRIPTION
  Restores, Builds and runs tests using dotnet and filtering of scope.
  .EXAMPLE
  Run-TestsWithFilter TestMethodName -restore -build
  .EXAMPLE
  Run-TestsWithFilter TestMethodName -b
  .PARAMETER filter
  The filter to be passed to dotnet test --filter option. No filter will run all tests.
  .PARAMETER restore
  Restores the project before running tests.
  .PARAMETER build
  Builds the project before running tests.
  #>
    [CmdletBinding()]
    param
    (
        [Alias('f')]
        [string]$filter,
        [Alias('r')]
        [switch]$restore,
        [Alias('b')]
        [switch]$build
    )

    if ($restore) {
        Write-Host "msbuild /v:m /m /t:restore"
        & msbuild /v:m /m /t:restore
    }

    if ($build) {
        Write-Host "msbuild /v:m /m"
        & msbuild /v:m /m
    }

    if ([string]::IsNullOrEmpty($filter)) {
        Write-Host "dotnet test --no-build --no-restore"
        & dotnet test --no-build --no-restore
    }
    else {
        Write-Host "dotnet test --no-build --no-restore --filter DisplayName~$filter"
        & dotnet test --no-build --no-restore --filter DisplayName~$filter
    }
}

Function Patch-CLI {
    param
    (
        [string]$sdkLocation
    )

    if([string]::IsNullOrEmpty($sdkLocation)){
        # F:\Forks\cli\bin\2\win-x64\dotnet\sdk\2.1.400-preview-008853
        $builtInCLI = [System.IO.Path]::Combine($cliRoot, 'bin', '2', 'win-x64', 'dotnet', 'sdk')
        Write-Host "Using the preconfigured CLI location: $builtInCLI"

        if (-Not (Test-Path $builtInCLI)) {
            Write-Error "The CLI path $builtInCLI does not exist!"
            return;
        }
        $sdk_path = (Get-ChildItem $builtInCLI)[0].FullName

        if(-Not (Test-Path $sdk_path)){
            Write-Error "The SDK path does not exist $sdk_path"
        }
    } 
    else 
    {
        if (-Not (Test-Path $sdkLocation)) {
            Write-Error "The SDK path $sdkLocation does not exist!"
            return;
        }
        $sdk_path = $sdkLocation
    }
    
    $nugetXplatArtifactsPath = [System.IO.Path]::Combine($nugetClientRoot, 'artifacts', 'NuGet.CommandLine.XPlat', '16.0', 'bin', 'Debug', 'netcoreapp2.1')
    $nugetBuildTasks = [System.IO.Path]::Combine($nugetClientRoot, 'artifacts', 'NuGet.Build.Tasks', '16.0', 'bin', 'Debug', 'netstandard2.0', 'NuGet.Build.Tasks.dll')
    $nugetTargets = [System.IO.Path]::Combine($nugetClientRoot, 'src', 'NuGet.Core', 'NuGet.Build.Tasks', 'NuGet.targets')

    if (-Not (Test-Path $nugetXplatArtifactsPath)) {
        Write-Error "$nugetXplatArtifactsPath not found!"
        return;
    }

    if (-Not (Test-Path $nugetBuildTasks)) {
        Write-Error "$nugetBuildTasks not found!"
        return;
    }

    if (-Not (Test-Path $nugetTargets)) {
        Write-Error "$nugetTargets not found!"
        return;
    }
 
    Write-Host
    Write-Host "Source commandline path - $nugetXplatArtifactsPath"
    Write-Host "Destination sdk path - $sdk_path"
    Write-Host
    
    Get-ChildItem $nugetXplatArtifactsPath -Filter NuGet*.dll | 
        Foreach-Object {	
            $new_position = "$($sdk_path)\$($_.BaseName )$($_.Extension )"
                
            Write-Host "Moving to - $($new_position)"
            Copy-Item $_.FullName $new_position
        }

    $buildTasksDest = "$($sdk_path)\NuGet.Build.Tasks.dll" 
    Write-Host "Moving to - $($buildTasksDest)"
    Copy-Item $nugetBuildTasks $buildTasksDest

    $nugetTargetsDest = "$($sdk_path)\NuGet.targets" 
    Write-Host "Moving to - $($nugetTargetsDest)"
    Copy-Item $nugetTargets $nugetTargetsDest
}

Function Run-Expression {
    param
    (
        [string]$expression
    )
    Write-Host "$expression"
    Invoke-Expression $expression
}

Function Build-NuGet {
    Run-Expression "$($nugetClientRoot)\build.ps1 -SkipUnitTest"
}

Function Build-CLI {
    Run-Expression "$($cliRoot)\build.cmd /p:CLIBUILD_SKIP_TESTS=true"
}

