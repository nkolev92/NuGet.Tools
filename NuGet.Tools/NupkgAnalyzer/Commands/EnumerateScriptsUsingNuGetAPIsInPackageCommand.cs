using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using NuGet.Protocol;
using System.Linq;
using System.IO;

namespace NupkgAnalyzer
{
    public class EnumerateScriptsUsingNuGetAPIsInPackageCommand : IProcessNupkgCommand
    {
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
                    var path = Path.GetTempFileName();
                    scriptFile.ExtractToFile(path, true);

                    using (var stream = scriptFile.Open())
                    using (var reader = new StreamReader(stream))
                    {
                        var scriptFileContent = reader.ReadToEnd();
                        if (scriptFileContent.Contains("NuGet.VisualStudio.IFileSystemProvider") ||
                            scriptFileContent.Contains("NuGet.VisualStudio.ISolutionManager"))
                        {
                            ps1FilesContainingNuGetAPIs.Add(scriptFile.FullName);
                        }
                    }
                }
            }

            results.Add(Constants.PS1ScriptsWithNuGetAPIs, string.Join(";", ps1FilesContainingNuGetAPIs));
            return results;
        }
    }
}
