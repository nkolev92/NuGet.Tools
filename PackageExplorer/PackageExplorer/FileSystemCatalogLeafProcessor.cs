using Knapcode.MiniZip;
using Microsoft.Extensions.Logging;
using NuGet.Protocol.Catalog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PackageExplorer
{
    class FileSystemCatalogLeafProcessor : ICatalogLeafProcessor
    {
        private readonly ILogger<FileSystemCatalogLeafProcessor> _logger;
        private readonly PackageInfoFactory _packageInfoFactory;
        private static HttpClient httpClient = new HttpClient();
        private readonly PackagePersistenceUtility _packagePersistenceUtility;
        public FileSystemCatalogLeafProcessor(PackageInfoFactory packageInfoFactory, PackagePersistenceUtility packagePersistenceUtility, ILogger<FileSystemCatalogLeafProcessor> logger)
        {

            _packageInfoFactory = packageInfoFactory ?? throw new ArgumentNullException(nameof(packageInfoFactory));
            _packagePersistenceUtility = packagePersistenceUtility ?? throw new ArgumentNullException(nameof(packagePersistenceUtility));
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
            var httpZipProvider = new HttpZipProvider(httpClient);
            var nupkgExists = File.Exists(packageInfo.LocalNupkgPath);
            var nuspecExists = File.Exists(packageInfo.LocalNuspecPath);
            if (nupkgExists && nuspecExists)
            {
                return true;
            }

            if (!await _packagePersistenceUtility.StorePackageOnDiskAsync(httpZipProvider, packageInfo.ContentUri, packageInfo))
            {
                return false;
            }
            if (!await _packagePersistenceUtility.StoreNuspecOnDiskAsync(httpClient, packageInfo.NuspecUri, packageInfo))
            {
                return false;
            }

            return true;
        }
    }
}
