using Knapcode.MiniZip;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Catalog;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace PackageExplorer
{
    partial class Program
    {

        public static async Task<int> Main(string[] args)
        {
            var downloadLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "downloads");

            if (args.Length > 0)
            {
                // Setting the download location to [args[0
                downloadLocation = args[0];
            }

            using (var loggerFactory = new LoggerFactory().AddConsole(LogLevel.Warning))
            using (var httpClient = new HttpClient())
            {
                var logger = loggerFactory.CreateLogger<Program>();

                var simpleHttpClient = new SimpleHttpClient(httpClient, loggerFactory.CreateLogger<SimpleHttpClient>());
                var fileCursor = new FileCursor("cursor.json", loggerFactory.CreateLogger<FileCursor>());
                var catalogClient = new CatalogClient(simpleHttpClient, loggerFactory.CreateLogger<CatalogClient>());
                VersionFolderPathResolver versionFolderPathResolver = new VersionFolderPathResolver(downloadLocation);

                var factory = new PackageInfoFactory(Repository.Factory.GetCoreV3(NuGetConstants.V3FeedUrl), versionFolderPathResolver);
                var packagePersistenceUtility = new PackagePersistenceUtility(loggerFactory.CreateLogger<PackagePersistenceUtility>());

                var leafProcessor = new CompatibilityAnalyzingCatalogLeafProcessor(factory, loggerFactory.CreateLogger<CompatibilityAnalyzingCatalogLeafProcessor>());
                var settings = new CatalogProcessorSettings
                {
                    MinCommitTimestamp = DateTimeOffset.UtcNow.AddDays(-300),
                    MaxCommitTimestamp = DateTimeOffset.UtcNow.AddDays(-299),
                    ExcludeRedundantLeaves = false,
                };

                var catalogProcessor = new CatalogProcessor(
                    fileCursor,
                    catalogClient,
                    leafProcessor,
                    settings,
                    loggerFactory.CreateLogger<CatalogProcessor>());

                int consecutiveFailures = 0;
                int count = 0;
                do
                {
                    var success = await catalogProcessor.ProcessAsync();
                    if (!success)
                    {
                        consecutiveFailures++;
                        logger.LogWarning($"Processing the catalog leafs failed. Retrying. {consecutiveFailures}");
                    }
                    else
                    {
                        consecutiveFailures = 0;
                    }
                    count++;
                }
                while (consecutiveFailures < 3 && count < 30);
            }
            return 0;
        }

        public static async Task<IEnumerable<string>> GetPackageFileListFromFlatContainerAsync(HttpZipProvider httpZipProvider, Uri url, Func<IEnumerable<string>, IEnumerable<string>> filter = null)
        {
            using (var zipDirectoryReader = await httpZipProvider.GetReaderAsync(url))
            {
                var zipDirectory = await zipDirectoryReader.ReadAsync();
                var entries = zipDirectory.Entries.Select(e => e.GetName());
                if (filter != null)
                {
                    entries = filter.Invoke(entries);
                }

                return entries;
            }
        }
    }
}
