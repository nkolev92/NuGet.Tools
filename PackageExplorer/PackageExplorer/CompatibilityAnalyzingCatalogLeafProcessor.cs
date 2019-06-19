using Knapcode.MiniZip;
using Microsoft.Extensions.Logging;
using NuGet.Packaging;
using NuGet.Protocol.Catalog;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PackageExplorer
{
    class CompatibilityAnalyzingCatalogLeafProcessor : ICatalogLeafProcessor
    {
        private readonly ILogger<CompatibilityAnalyzingCatalogLeafProcessor> _logger;
        private readonly PackageInfoFactory _packageInfoFactory;
        private static HttpClient httpClient = new HttpClient();
        public CompatibilityAnalyzingCatalogLeafProcessor(PackageInfoFactory packageInfoFactory, ILogger<CompatibilityAnalyzingCatalogLeafProcessor> logger)
        {

            _packageInfoFactory = packageInfoFactory ?? throw new ArgumentNullException(nameof(packageInfoFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<bool> ProcessPackageDeleteAsync(PackageDeleteCatalogLeaf leaf)
        {
            _logger.LogInformation(
                $"{leaf.CommitTimestamp:O}: Found package delete leaf for {leaf.PackageId} {leaf.PackageVersion}.");

            return GetResultAsync(leaf);
        }

        public Task<bool> ProcessPackageDetailsAsync(PackageDetailsCatalogLeaf leaf)
        {
            _logger.LogInformation(
                $"{leaf.CommitTimestamp:O}: Found package details leaf for {leaf.PackageId} {leaf.PackageVersion}.");

            return GetResultAsync(leaf);
        }

        private async Task<bool> GetResultAsync(ICatalogLeafItem leaf)
        {
            var packageInfo = await _packageInfoFactory.CreatePackageInfo(leaf.PackageId, leaf.PackageVersion);
            

            using (var nupkgStream = await httpClient.GetStreamAsync(packageInfo.ContentUri))
            using (var packageArchiveReader = new PackageArchiveReader(nupkgStream))
            {
                var nuspecReader = packageArchiveReader.NuspecReader;

                var analyzedObject = new PackageCompatibilityInfo(leaf.PackageId, leaf.PackageVersion);
                
                var fileList = packageArchiveReader.GetFiles().Where(e => ShouldInclude(e));
                analyzedObject.AnalyzePackageContentCompatibility(fileList);
                analyzedObject.AnalyzePackageDependenciesCompatibility(nuspecReader);

                _logger.LogWarning(analyzedObject.ToString());

            }
            return true;
        }


        // Not all the files from a zip file are needed
        // So, files such as '.rels' and '[Content_Types].xml' are not relevant
        private static bool ShouldInclude(string fullName)
        {
            var fileName = Path.GetFileName(fullName);
            if (fileName != null)
            {
                if (fileName == ".rels")
                {
                    return false;
                }
                if (fileName == "[Content_Types].xml")
                {
                    return false;
                }
            }

            var extension = Path.GetExtension(fullName);
            if (extension == ".psmdcp")
            {
                return false;
            }
            return true;
        }
    }
}
