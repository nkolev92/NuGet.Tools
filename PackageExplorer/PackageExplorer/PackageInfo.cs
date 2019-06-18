using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;

namespace PackageExplorer
{
    internal class PackageInfo
    {

        public string PackageId { get; }

        public string PackageVersion { get; }

        public string Path { get; set; }

        public Uri ContentUri { get; set; } // Comes from flat container

        public Uri NuspecUri { get; set; } // Comes from registration.

        private Lazy<PackageIdentity> packageIdentity;

        public PackageInfo(string packageId, string packageVersion, string path, Uri contentUri, Uri nuspecUri)
        {
            PackageId = packageId;
            PackageVersion = packageVersion;
            Path = path;
            ContentUri = contentUri;
            NuspecUri = nuspecUri;
        }

        public PackageIdentity PackageIdentity
        {
            get
            {
                if (packageIdentity == null)
                {
                    packageIdentity = new Lazy<PackageIdentity>(() => new PackageIdentity(PackageId, NuGetVersion.Parse(PackageVersion)));
                }
                return packageIdentity?.Value;
            }
        }
    }
}
