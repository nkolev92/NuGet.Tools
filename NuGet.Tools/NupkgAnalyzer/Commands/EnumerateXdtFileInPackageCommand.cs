using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NuGet.Protocol;

namespace NupkgAnalyzer
{
    class EnumerateXdtFileInPackageCommand : IProcessNupkgCommand
    {
        public Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage)
        {
            var results = new Dictionary<string, string>(); ;

            var xdtFiles = archive.Entries.Where(e => e.FullName.EndsWith(".xdt", StringComparison.OrdinalIgnoreCase));

            results.Add(Constants.XdtFiles, string.Join(";", xdtFiles));
            return results;
        }

    }

}
