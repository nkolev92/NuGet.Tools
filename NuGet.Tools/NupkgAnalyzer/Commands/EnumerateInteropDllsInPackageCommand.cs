using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NuGet.Protocol;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NupkgAnalyzer
{
    class EnumerateInteropDllsInPackageCommand : IProcessNupkgCommand
    {
        private string _tempeExtractionPath;

        public EnumerateInteropDllsInPackageCommand(string tempExtractionPath)
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

            var interopFiles = new List<string>();
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.EndsWith("dll", StringComparison.OrdinalIgnoreCase) || entry.FullName.EndsWith("exe", StringComparison.OrdinalIgnoreCase))
                {
                    var path = GetRandomPath();
                    entry.ExtractToFile(path, true);
                    var assembly = Assembly.LoadFrom(path);  // ReflectionOnly load is not available everywhere
                    if (IsCOMAssembly(assembly))
                    {
                        interopFiles.Add(entry.ToString());
                    }
                }
            }
            results.Add(Constants.InteropFiles, string.Join(";", interopFiles));
            return results;
        }

        private static bool IsCOMAssembly(Assembly a)
        {
            object[] AsmAttributes = a.GetCustomAttributes(typeof(ImportedFromTypeLibAttribute), true);
            if (AsmAttributes.Length > 0)
            {
                return true;
            }
            return false;
        }
    }
}
