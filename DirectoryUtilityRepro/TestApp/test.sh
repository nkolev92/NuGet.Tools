#!/bin/bash

usage() { echo "Usage: $0 [-r <string>] [-c <int> ] [-o <bool> ] [-v <2|3> ]" 1>&2; exit 1; }

dir=$(cd "${0%[/\\]*}" > /dev/null && pwd)

while getopts ":r:c:o:v:" opt; do
  case $opt in
    r)
	  rootDirectory=$OPTARG
      ;;
	c)
	  concurrentTasks=$OPTARG
      ;;
	o)
	  originalBehavior=$OPTARG
	  ;;
	v)
	  version=$OPTARG
	  ;;
    \?)
      echo "Invalid option: -$OPTARG" >&2
      exit 1
      ;;
    :)
      echo "Option -$OPTARG requires an argument." >&2
      exit 1
      ;;
  esac
done

if [ -z "$rootDirectory" ]; then
rootDirectory=$dir
fi

if [ -z "$concurrentTasks" ]; then
concurrentTasks=7
fi

if [ -z "$originalBehavior" ]; then
originalBehavior=true
fi

if [ -z "$originalBehavior" ]; then
version=3
fi

if [[ $version -eq 2 ]]; then
sdkLink=https://dotnetcli.blob.core.windows.net/dotnet/Sdk/release/2.2.2xx/dotnet-sdk-latest-linux-x64.tar.gz
targetFramework=netcoreapp2.2
else 
sdkLink=https://dotnetcli.azureedge.net/dotnet/Sdk/3.0.100-preview4-010944/dotnet-sdk-3.0.100-preview4-010944-linux-x64.tar.gz
targetFramework=netcoreapp3.0
fi

echo "SDK=$sdkLink"
echo "rootDirectory=$rootDirectory"
echo "concurrentTasks=$concurrentTasks"
echo "originalBehavior=$originalBehavior"

mkdir -p $rootDirectory

if [ -d "$rootDirectory/dotnet-sdk" ]; then
	echo "The dotnet-sdk is already downloaded."
else 
	wget -O $rootDirectory/dotnet-sdk.tar.gz $sdkLink
	mkdir -p "$rootDirectory/dotnet-sdk"
	tar -xzf "$rootDirectory/dotnet-sdk.tar.gz" --directory "$rootDirectory/dotnet-sdk"
	echo "Done setting up the dotnet sdk"
fi

echo "Building the project: $rootDirectory/dotnet-sdk/dotnet build $dir/TestApp.csproj"

$rootDirectory/dotnet-sdk/dotnet build $dir/TestApp.csproj /p:TargetFramework=$targetFramework


let VAL=0
let iteration=0

while [ $VAL -eq 0 ]

do

echo "deleting the $rootDirectory/testDir directory"
rm -rf "$rootDirectory/testDir"

let iteration+=1
echo "running dotnet $iteration with $rootDir and concurrenttasks $concurrentTasks"
$rootDirectory/dotnet-sdk/dotnet exec $dir/bin/Debug/$targetFramework/TestApp.dll $rootDirectory/testDir $concurrentTasks $originalBehavior
let VAL=$?

done

echo "done $iteration" 
echo "There was a failure."