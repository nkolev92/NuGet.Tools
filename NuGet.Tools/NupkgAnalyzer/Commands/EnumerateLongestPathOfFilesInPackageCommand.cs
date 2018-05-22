using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Protocol;

namespace NupkgAnalyzer
{
    class EnumerateLongestPathOfFilesInPackageCommand : IProcessNupkgCommand
    {
        public Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage)
        {
            var results = new Dictionary<string, string>(); ;

            int maxLength = 0;

            foreach (var entry in archive.Entries)
            {
                if (maxLength < entry.FullName.Length)
                {
                    maxLength = entry.FullName.Length;
                }
            }

            var sb = new StringBuilder();
            foreach (var type in localPackage.Nuspec.GetPackageTypes())
            {
                sb.Append(type.Name);
                sb.Append(type.Version.ToString());
                sb.Append(", ");
            }

            results.Add(Constants.LongestPathInNupkg, maxLength + "");
            results.Add(Constants.PackageTypes, sb.ToString());

            return results;
        }
    }
}
