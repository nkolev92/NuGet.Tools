using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NupkgAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string nupkgsPath = @"E:\UniquePackages";
            string outputPath = @"E:\Results";

            var analyzer = new PackageAnalyzer(nupkgsPath, NuGetDirectoryStructure.V3, Path.GetTempPath());

            var commands = new List<IProcessNupkgCommand>() {
//                new EnumeratePPFilesInPackageCommand(),
//                new EnumeratePS1ScriptsInPackageCommand(),
//                new EnumerateScriptsUsingNuGetAPIsInPackageCommand(@"E:\data"),
//                new EnumerateContentFilesInPackageCommand()
//                new EnumerateXdtFileInPackageCommand(),
//                  new EnumerateInteropDllsInPackageCommand(@"E:\data"),
                  new EnumerateTargetsFileInPackageCommand()
            };
            var beforeRunCommand = DateTime.Now;
            var results = analyzer.ExecuteCommands(commands);
            var afterRunCommand = DateTime.Now;

            var values = new List<List<string>>();

            var names = new List<string>() { Constants.ID, Constants.Version, Constants.TargetFiles };

            values.Add(names);
            foreach (var dict in results)
            {
                var row = new List<string>();
                foreach (var key in names)
                {
                    string value;
                    dict.TryGetValue(key, out value);
                    row.Add(value);
                }
                values.Add(row);
            }

            var csv = new StringBuilder();

            foreach (var row in values)
            {
                csv.Append(string.Join(",", row)).Append("\r\n");
            }

            var afterProcessing = DateTime.Now;

            File.WriteAllText(Path.Combine(outputPath, "PackagesWithTargets.csv"), csv.ToString());

            var stats = new StringBuilder();
            stats.Append("Before command = ").Append(beforeRunCommand.ToLongTimeString());
            stats.Append("After command = ").Append(afterRunCommand.ToLongTimeString());
            stats.Append("After Processing = ").Append(afterProcessing.ToLongTimeString());

            File.WriteAllText(Path.Combine(outputPath, "stats.txt"),stats.ToString());

        }
    }
}
