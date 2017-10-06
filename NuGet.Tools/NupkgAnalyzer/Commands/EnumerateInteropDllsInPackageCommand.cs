using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NuGet.Protocol;

namespace NupkgAnalyzer
{
    class EnumerateInteropDllsInPackageCommand : IProcessNupkgCommand
    {
        public Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage)
        {
            var results = new Dictionary<string, string>(); ;

            var interopFiles = archive.Entries.Where(e => e.FullName.IndexOf("Interop", StringComparison.OrdinalIgnoreCase) >= 0);

            results.Add(Constants.InteropFiles, string.Join(";", interopFiles));
            return results;
        }
    }
}
