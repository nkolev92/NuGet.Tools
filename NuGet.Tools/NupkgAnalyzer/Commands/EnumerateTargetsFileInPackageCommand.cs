using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NupkgAnalyzer
{
    public class EnumerateTargetsFileInPackageCommand : IProcessNupkgCommand
    {
        public Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage)
        {
            var targetFiles = archive.Entries
                              .Where(e => e.FullName.EndsWith(localPackage.Identity.Id + ".targets", StringComparison.OrdinalIgnoreCase));

            var results = new Dictionary<string, string>(); ;

            results.Add(Constants.TargetFiles, string.Join(";", targetFiles));
            return results;
        }
    }
}
