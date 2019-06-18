using Knapcode.MiniZip;
using NuGet.CatalogReader;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
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

        public static async Task<int> Main()
        {

            PackageInfoFactory packageInfoFactory = new PackageInfoFactory(Repository.Factory.GetCoreV3(NuGetConstants.V3FeedUrl));

            var packageId = "Newtonsoft.Json";
            var packageVersion = "10.0.3";
            //bool beDone = false; // TODO NK - This is crazy slow. Think of an alternative approach. Consider walking bit by bit. 
            //using (var catalog = new CatalogReader(new Uri(NuGetConstants.V3FeedUrl)))
            //{
            //    foreach (var entry in await catalog.GetFlattenedEntriesAsync(workDay, DateTimeOffset.Now))
            //    {
            //        Console.WriteLine($"[{entry.CommitTimeStamp}] {entry.Id} {entry.Version}");
            //        if (beDone)
            //        {
            //            throw new Exception();
            //        }
            //        beDone = true;
            //    }
            //}

            var packageInfo = await packageInfoFactory.CreatePackageInfo(packageId, packageVersion);
            // Use nuspecUri to get the nuspec. packageInfo.NuspecUri 
            using (var httpClient = new HttpClient())
            {
                VersionFolderPathResolver versionFolderPathResolver = new VersionFolderPathResolver(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "downloads"));
                var httpZipProvider = new HttpZipProvider(httpClient);
                var packagePath = Path.Combine(versionFolderPathResolver.GetInstallPath(packageId, packageVersion), "package.mzip");
                if (await PackagePersistenceUtility.StorePackageOnDiskAsync(httpZipProvider, packageInfo.ContentUri, packagePath))
                {
                    var zip = await PackagePersistenceUtility.ReadPackageFromDisk(packagePath);

                    foreach (var entry in zip.Entries.Select(e => e.GetName()))
                    {
                        Console.WriteLine(entry);
                    }
                }
                else
                {
                    return 1;
                }




                //Func<IEnumerable<string>, IEnumerable<string>> packageFileFilter = (list) => list.Where(e => ShouldInclude(e));
                //PrintList(await GetPackageFileListFromFlatContainerAsync(httpZipProvider, packageInfo.ContentUri, packageFileFilter));
            }
            return 0;
        }

        private static void PrintList(IEnumerable<string> list)
        {
            foreach (var entry in list)
            {
                Console.WriteLine(entry);
            }
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
