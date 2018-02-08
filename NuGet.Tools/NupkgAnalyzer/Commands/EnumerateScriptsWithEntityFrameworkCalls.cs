using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NupkgAnalyzer
{

    class EnumerateScriptsWithEntityFrameworkCalls : IProcessNupkgCommand
    {
        private string _tempeExtractionPath;
        public EnumerateScriptsWithEntityFrameworkCalls(string tempExtractionPath)
        {
            _tempeExtractionPath = tempExtractionPath;
        }

        private string GetRandomPath()
        {
            return Path.Combine(_tempeExtractionPath, Guid.NewGuid().ToString());
        }

        public Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage)
        {

            var results = new Dictionary<string, string>(); ;

            var ps1Files = archive.Entries
                    .Where(e => e.FullName.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                    .ToArray();

            var ps1FilesContainingNuGetAPIs = new List<string>();

            if (ps1Files.Count() > 0)
            {

                foreach (var scriptFile in ps1Files)
                {
                    var path = GetRandomPath();
                    scriptFile.ExtractToFile(path, true);

                    using (var stream = scriptFile.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var scriptFileContent = reader.ReadToEnd();
                        if (scriptFileContent.Contains("Add-Migration") ||
                            scriptFileContent.Contains("Add-EFProvider") ||
                            scriptFileContent.Contains("Initialize-EFConfiguration") ||
                            scriptFileContent.Contains("Update-Database"))
                        {
                            ps1FilesContainingNuGetAPIs.Add(scriptFile.FullName);
                        }
                    }
                }
            }

            results.Add(Constants.PS1ScriptsWithEFAPIs, string.Join(";", ps1FilesContainingNuGetAPIs));
            return results;
        }
    }

}
