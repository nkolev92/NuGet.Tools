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

            var builder = new StringBuilder(); ;
            foreach(var dict in results)
            {
                foreach (var kvp in dict)
                {
                    builder.Append($"{kvp.Key} => {kvp.Value}; ");
                }
                builder.Append("\r\n");
            }
            File.WriteAllText(Path.Combine(nupkgsPath,"results.txt"), builder.ToString());
        }
    }
}
