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
            string nupkgsPath = @"C:\Users\Roki2\Documents\Code\NuGet\NuGet.Client\artifacts\nupkgs";

            var analyzer = new PackageAnalyzer(nupkgsPath, NuGetDirectoryStructure.V2, Path.GetTempPath());

            var commands = new List<IProcessNupkgCommand>() {
                new EnumeratePPFilesInPackageCommand(),
                new EnumeratePS1ScriptsInPackageCommand(),
                new EnumerateScriptsUsingNuGetAPIsInPackageCommand(),
                new EnumerateContentFilesInPackageCommand() };

            var results = analyzer.ExecuteCommands(commands);

            var values = new List<List<string>>();

            var names = new List<string>() { Constants.ID, Constants.Version, Constants.PS1Scripts, Constants.PPFiles, Constants.ContentFiles, Constants.PS1ScriptsWithNuGetAPIs };

            values.Add(names);
            foreach (var dict in results)
            {
                var row = new List<string>();
                foreach (var key in names)
                {
                    row.Add(dict.GetValueOrDefault(key));
                }
                values.Add(row);
            }

            var csv = new StringBuilder();

            foreach (var row in values)
            {
                csv.Append(string.Join(",", row)).Append("\r\n");
            }

            File.WriteAllText(Path.Combine(nupkgsPath, "results.csv"), csv.ToString());
        }
    }
}
