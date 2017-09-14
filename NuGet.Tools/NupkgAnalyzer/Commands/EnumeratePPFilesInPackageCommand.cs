using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using NuGet.Protocol;
using System.Linq;

namespace NupkgAnalyzer
{
    class EnumeratePPFilesInPackageCommand : IProcessNupkgCommand
    {
        public Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage)
        {
            var results = new Dictionary<string, string>(); ;

            var ppFiles = archive.Entries.Where(e => e.FullName.EndsWith(".pp"));

            results.Add(Constants.PPFiles, string.Join(";", ppFiles));
            return results;
        }
    }
}
