using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PackagesConfigAnalysisTool
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Insufficient parameters. Please provide a path to a packages.config file and the project's target framework");
                Console.WriteLine(@"Example: .\PackagesConfigAnalysisTool.exe F:\Code\Samples\ProjectA\packages.config net472");
                return 1;
            }

            var packagesConfigPath = args[0];
            var targetFrameworkString = args[1];
            var source = NuGetConstants.V3FeedUrl;

            // Read the packages.config
            IEnumerable<PackageReference> packages = null;
            if (File.Exists(packagesConfigPath))
            {
                using (var packagesConfigStream = File.OpenRead(packagesConfigPath))
                {
                    var reader = new PackagesConfigReader(packagesConfigStream);
                    packages = reader.GetPackages();
                }

            }
            else
            {
                Console.WriteLine("The packages.config provided not exist. Path: " + packagesConfigPath);
                return 1;
            }

            // Ensure the tfm is valid. An invalid tfm will cause inconsistency in the results. 
            var targetFramework = NuGetFramework.Parse(targetFrameworkString);
            if (!targetFramework.IsSpecificFramework)
            {
                Console.WriteLine($"The framework provided {targetFrameworkString} is not a valid target framework");
                return 1;
            }

            try
            {
                Console.WriteLine($"Will use {source} to analyze the packages");

                // Get the resource to 
                var sourceRepository = Repository.Factory.GetCoreV3(source);
                var dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();

                // Define the caching context. Tell NuGet for how long it should cache the remote results (jsons). 
                var cacheContext = new SourceCacheContext();
                cacheContext.MaxAge = DateTimeOffset.Now.AddDays(-1); // cache for 1 day.

                var packageDependencyInfos = (await PackageGraphAnalysisUtilities.GetDependencyInfoForPackageIdentitiesAsync(
                    packageIdentities: packages.Select(e => e.PackageIdentity),
                    nuGetFramework: targetFramework,
                    dependencyInfoResource: dependencyInfoResource,
                    sourceCacheContext: cacheContext,
                    includeUnresolved: true, // This means that packages that can't be resolved will be included and treated as top level packages by the below logic.
                    logger: NullLogger.Instance,
                    cancellationToken: CancellationToken.None
                    ))
                    .ToList();

                var dependantPackages = PackageGraphAnalysisUtilities.GetPackagesWithDependants(packageDependencyInfos);

                Console.WriteLine("Top level packages:");
                Console.WriteLine();

                foreach (var package in dependantPackages.Where(e => e.IsTopLevelPackage))
                {
                    Console.WriteLine(package.Identity.Id + " " + package.Identity.Version);
                }

                Console.WriteLine();
                Console.WriteLine("Transitive packages:");
                Console.WriteLine();
                foreach (var package in dependantPackages.Where(e => !e.IsTopLevelPackage))
                {
                    Console.WriteLine(package.Identity.Id + " " + package.Identity.Version);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Encountered a problem analyzing the packages. " + e.ToString());
            }

            return 0;
        }
    }
}
