msbuild /restore .\PackagesConfigAnalysisTool/PackagesConfigAnalysisTool.csproj /p:Configuration="Release" /v:q

PackagesConfigAnalysisTool\bin\Release\net472\PackagesConfigAnalysisTool.exe .\TestPackagesConfigProject\packages.config "net472"