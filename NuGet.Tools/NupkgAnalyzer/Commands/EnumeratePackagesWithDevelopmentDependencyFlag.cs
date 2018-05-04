using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Protocol;

namespace NupkgAnalyzer
{
    class EnumeratePackagesWithDevelopmentDependencyFlagCommand : IProcessNupkgCommand
    {
        public Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage)
        {
            var results = new Dictionary<string, string>(); ;

            var nuspec = localPackage.Nuspec;
            if (nuspec.GetDevelopmentDependency()) { 
                results.Add(Constants.DevelopmentDependency, "Development Dependency");
            }
            return results;
        }
    }
}
