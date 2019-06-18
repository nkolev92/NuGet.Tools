using NuGet.Packaging;
using NuGet.Protocol;
using System;
using System.Globalization;
using System.IO;

namespace PackageExplorer
{
    public static class NuGetResourceExtensions
    {
        public static Uri GetUri(this RegistrationResourceV3 registrationResource, string packageId, string packageVersion)
        {
            if (packageId == null || packageVersion == null)
            {
                throw new InvalidOperationException();
            }

            return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}/{1}/{2}.json", registrationResource.BaseUri.AbsoluteUri.TrimEnd('/'),
                packageId.ToLowerInvariant(), packageVersion.ToLowerInvariant()));
        }

        public static string GetInstallPath(this VersionFolderPathResolver versionFolderPathResolver, string packageId, string version)
        {
            return Path.Combine(
                versionFolderPathResolver.RootPath,
                versionFolderPathResolver.GetVersionListDirectory(packageId),
                version);
        }
    }
}
