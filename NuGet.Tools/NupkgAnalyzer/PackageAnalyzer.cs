using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.IO.Compression;
using System.Linq;

namespace NupkgAnalyzer
{

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
            var logger = new Logger(NuGet.Common.LogLevel.Information);

            switch (nuGetDirectoryStructure)
            {
                case NuGetDirectoryStructure.V2:
                    return LocalFolderUtility.GetPackagesV2(root: rootPath, log: logger);
                case NuGetDirectoryStructure.V3:
                    return LocalFolderUtility.GetPackagesV3(root: rootPath, log: logger);
            }

            return new List<LocalPackageInfo>();
        }

        public List<Dictionary<string, string>> ExecuteCommands(IEnumerable<IProcessNupkgCommand> commands)
        {
            var result = new List<Dictionary<string, string>>();

            foreach (var package in GetAllPackagesInFolder(_nupkgsRootDirectory, _nuGetDirectoryStructure))
            {
                var commandResults = new Dictionary<string, string>();
                commandResults.Add("packageId", package.Identity.Id);
                commandResults.Add("version", package.Identity.Version.ToFullString());

                using (var archive = ZipFile.Open(package.Path, ZipArchiveMode.Read)){
                    foreach (var command in commands)
                    {
                        var commandResult = command.Execute(archive, package);
                        commandResult.ToList().ForEach(e => commandResults.Add(e.Key, e.Value));
                    }
                }

                result.Add(commandResults);
            }
            return result;
        }
    }
}
