using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Versioning;

namespace NupkgAnalyzer
{
    enum NuGetDirectoryStructure
    {
        V2, V3
    }

    public interface IProcessNupkgCommand
    {
        Dictionary<string, string> Act(LocalPackageInfo localPackage);
    }


    public class EnumeratePS1ScriptsInPackage : IProcessNupkgCommand
    {
        public Dictionary<string, string> Act( LocalPackageInfo localPackage)
        {

            var results = new Dictionary<string, string>(); ;
            using (var archive = ZipFile.Open(localPackage.Path, ZipArchiveMode.Read))
            {

            }

            return results;
        }

    }

    internal class PackageAnalyzer
    {
        private string _nupkgsRootDirectory;
        private NuGetDirectoryStructure _nuGetDirectoryStructure;
        private string _logOutputPath;

        public PackageAnalyzer(string nupkgsRootDirectory, NuGetDirectoryStructure nuGetDirectoryStructure, string logOutputPath)
        {
            _nupkgsRootDirectory = nupkgsRootDirectory;
            _nuGetDirectoryStructure = nuGetDirectoryStructure;
            _logOutputPath = logOutputPath;
        }

        IEnumerable<LocalPackageInfo> GetAllPackagesInFolder(string rootPath, NuGetDirectoryStructure nuGetDirectoryStructure)
        {
            switch (nuGetDirectoryStructure)
            {
                case NuGetDirectoryStructure.V2:
                    return LocalFolderUtility.GetPackagesV2(root: rootPath, log: null);
                case NuGetDirectoryStructure.V3:
                    return LocalFolderUtility.GetPackagesV3(root: rootPath, log: null);
            }

            return new List<LocalPackageInfo>();
        } 



    }
}
