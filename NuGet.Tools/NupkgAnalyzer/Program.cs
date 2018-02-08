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
//                new EnumerateInteropDllsInPackageCommand( @"E:\Data"),
//                new EnumerateTargetsFileInPackageCommand(),
//                new EnumerateLibFilePatternsInPackageCommand(),
                  new EnumeratePackagesWithDevelopmentDependencyFlagCommand()
            };
            var beforeRunCommand = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff");
            var results = analyzer.ExecuteCommands(commands);
            var afterRunCommand = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff");

            var values = new List<List<string>>();

            var names = new List<string>() { Constants.ID, Constants.Version, Constants.DevelopmentDependency };

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

            var afterProcessing = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff");

            File.WriteAllText(Path.Combine(outputPath, "packagesWithDevelopmentDependencyFlag.csv"), csv.ToString());

            var stats = new StringBuilder();
            stats.Append("Before command = ").Append(beforeRunCommand);
            stats.Append("After command = ").Append(afterRunCommand);
            stats.Append("After Processing = ").Append(afterProcessing);

            File.WriteAllText(Path.Combine(outputPath, "stats.txt"),stats.ToString());

        }
    }
}
