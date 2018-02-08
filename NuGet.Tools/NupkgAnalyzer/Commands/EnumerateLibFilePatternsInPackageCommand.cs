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
    class EnumerateLibFilePatternsInPackageCommand : IProcessNupkgCommand
    {
        public Dictionary<string, string> Execute(ZipArchive archive, LocalPackageInfo localPackage)
        {

            var libFiles = EnumerateLibFilePattern(archive);

            var results = new Dictionary<string, string>(); ;

            results.Add(Constants.LibFilePatterns, string.Join(";", libFiles));
            return results;
        }

        public IEnumerable<string> EnumerateLibFilePattern(ZipArchive archive)
        {
            foreach(var entry in archive.Entries)
            {
                var name = entry.FullName.Split(Path.AltDirectorySeparatorChar);

                if(name.Length == 2 && name[0].Equals("lib", StringComparison.OrdinalIgnoreCase) &&
                   (name[1].EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || 
                    name[1].EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || 
                    name[1].EndsWith(".winmd", StringComparison.OrdinalIgnoreCase)))
                {
                    yield return entry.FullName;
                }
            }
        }
    }
}
