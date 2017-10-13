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

        private List<string> interopDlls = new List<string>();
        private List<string> allDlls = new List<string>();
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
                    if (!allDlls.Contains(entry.Name))
                    {
                        allDlls.Add(entry.Name);
                        var path = GetRandomPath();
                        entry.ExtractToFile(path, true);

                        AppDomain myDomain = AppDomain.CreateDomain("MyDomain");
                        byte[] data = System.IO.File.ReadAllBytes(path);
                        Assembly assembly = myDomain.Load(data);

                        if (IsCOMAssembly(assembly))
                        {
                            interopDlls.Add(entry.Name);
                            interopFiles.Add(entry.ToString());
                        }
                        AppDomain.Unload(myDomain);
                    }
                    else
                    {
                        if (interopDlls.Contains(entry.Name))
                        {
                            interopFiles.Add(entry.ToString());
                        }
                    }
                }
            }
            results.Add(Constants.InteropFiles, string.Join(";", interopFiles));
            return results;
        }

        private static bool IsCOMAssembly(Assembly a)
        {
            try
            {
                object[] AsmAttributes = a.GetCustomAttributes(typeof(ImportedFromTypeLibAttribute), true);
                if (AsmAttributes.Length > 0)
                {
                    return true;
                }
            }
            catch (FileNotFoundException e)
            {
                return false;
            }
            return false;
        }
    }
}
