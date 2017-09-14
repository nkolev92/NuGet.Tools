using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using NuGet.Protocol;

namespace NupkgAnalyzer
{
    public class EnumeratePS1ScriptsInPackageCommand : IProcessNupkgCommand
    {
        public Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage)
        {

            var results = new Dictionary<string, string>(); ;

            var ps1Files = archive.Entries
                    .Where(e => e.FullName.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.FullName)
                    .ToArray();

            results.Add(Constants.PS1Scripts, string.Join(";", ps1Files));

            return results;
        }

    }
}
