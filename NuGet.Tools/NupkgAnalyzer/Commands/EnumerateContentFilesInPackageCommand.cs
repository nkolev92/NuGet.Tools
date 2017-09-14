using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using NuGet.Protocol;
using System.Linq;

namespace NupkgAnalyzer
{
    public class EnumerateContentFilesInPackageCommand : IProcessNupkgCommand
    {
        public Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage)
        {
            var contentFiles = archive.Entries
                              .Where(e => e.FullName.StartsWith("contentfiles", StringComparison.OrdinalIgnoreCase));

            var results = new Dictionary<string, string>(); ;

            results.Add(Constants.ContentFiles, string.Join(";", contentFiles));
            return results;
        }
    }
}
