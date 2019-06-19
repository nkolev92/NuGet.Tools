using NuGet.Client;
using NuGet.ContentModel;
using NuGet.Frameworks;
using NuGet.Packaging;
using System.Collections.Generic;
using System.Linq;

namespace PackageExplorer
{

    public static class PackageAssetsCompatibilityAnalyzer
    {

        public static void AnalyzePotentialTargetFrameworkInconsistencies(this PackageCompatibilityInfo analyzedPackage)
        {
            if (analyzedPackage.DependencyGroupFrameworks != null && analyzedPackage.LibFrameworks != null && analyzedPackage.RefFrameworks != null)
            {
                var libRefCombined = analyzedPackage.LibFrameworks.Union(analyzedPackage.RefFrameworks);

                if (!Enumerable.SequenceEqual(analyzedPackage.DependencyGroupFrameworks, libRefCombined)) // If all frameworks are not consistent
                {
                    if (analyzedPackage.DependencyGroupFrameworks.Any() && libRefCombined.Any()) // if either has no values, then there'd be no problems. This can be common for packages without dependencies. 
                    {
                        if (!(analyzedPackage.DependencyGroupFrameworks.Count() == 1 && analyzedPackage.DependencyGroupFrameworks.Contains("any"))) // If there's only 1 dependencies lemenet and it's any. Then there's no incompatibility. It's badly authored, but not an issue.
                        {
                            if (!(analyzedPackage.DependencyGroupFrameworks.Count() == 1 && libRefCombined.Count() == 1 && libRefCombined.Contains("net"))) // If there's only 1 lib ref and it's lib/*.dll. Then people do weird things. No need to flag them as they won't cause problems :)
                            {
                                analyzedPackage.PotentialCompatibilityInconsistency = true;
                            }
                        }   
                    }
                }

                if (analyzedPackage.DependencyGroupFrameworks.Contains("any"))
                {
                    analyzedPackage.DependencyGroupContainsAny = true;
                }
            }
        }

        public static void AnalyzePackageDependenciesCompatibility(this PackageCompatibilityInfo analyzedPackage, NuspecReader nuspecReader)
        {
            if (nuspecReader != null)
            {
                analyzedPackage.DependencyGroupFrameworks = nuspecReader.GetDependencyGroups().Select(e => e.TargetFramework.GetShortFolderName()).OrderBy(e => e);
            }
        }

        public static void AnalyzePackageContentCompatibility(this PackageCompatibilityInfo analyzedPackage, IEnumerable<string> fileList)
        {
            if (fileList != null)
            {
                var managedCodeConventions = new ManagedCodeConventions(new NuGet.RuntimeModel.RuntimeGraph());
                var collection = new ContentItemCollection();
                collection.Load(fileList);

                analyzedPackage.AllFiles = fileList;

                var libItems = GetContentForPattern(collection, managedCodeConventions.Patterns.CompileLibAssemblies);
                analyzedPackage.LibFrameworks = GetGroupFrameworks(libItems);
                analyzedPackage.LibFiles = GetGroupFiles(libItems);

                var refItems = GetContentForPattern(collection, managedCodeConventions.Patterns.CompileRefAssemblies);
                analyzedPackage.RefFrameworks = GetGroupFrameworks(refItems);
                analyzedPackage.RefFiles = GetGroupFiles(refItems);

                var runtimeItems = GetContentForPattern(collection, managedCodeConventions.Patterns.RuntimeAssemblies);
                analyzedPackage.RuntimeFrameworks = GetGroupFrameworks(runtimeItems);
                analyzedPackage.RuntimeFiles = GetGroupFiles(runtimeItems);

                var msbuildItems = GetContentForPattern(collection, managedCodeConventions.Patterns.MSBuildFiles);
                analyzedPackage.MSBuildFrameworks = GetGroupFrameworks(msbuildItems);
                analyzedPackage.MSBuildFiles = GetGroupFiles(msbuildItems);

                var msbuildMultiTargetingItems = GetContentForPattern(collection, managedCodeConventions.Patterns.MSBuildMultiTargetingFiles);
                analyzedPackage.MSBuildMultiTargetingFrameworks = GetGroupFrameworks(msbuildMultiTargetingItems);
                analyzedPackage.MSBuildMultiTargetingFiles = GetGroupFiles(msbuildMultiTargetingItems);

                var msbuildTransitiveItems = GetContentForPattern(collection, managedCodeConventions.Patterns.MSBuildTransitiveFiles);
                analyzedPackage.MSBuildTransitiveFrameworks = GetGroupFrameworks(msbuildTransitiveItems);
                analyzedPackage.MSBuildTransitiveFiles = GetGroupFiles(msbuildTransitiveItems);

                var anyItems = GetContentForPattern(collection, managedCodeConventions.Patterns.AnyTargettedFile);
                analyzedPackage.AnyTargetFrameworks = GetGroupFrameworks(anyItems);
                analyzedPackage.AnyTargetFiles = GetGroupFiles(anyItems);
            }
        }

        private static IEnumerable<ContentItemGroup> GetContentForPattern(ContentItemCollection collection, PatternSet pattern)
        {
            return collection.FindItemGroups(pattern)
                .OrderBy(group => ((NuGetFramework)group.Properties["tfm"]).GetShortFolderName());
        }

        private static IEnumerable<string> GetGroupFiles(IEnumerable<ContentItemGroup> groups)
        {
            return groups.SelectMany(e => e.Items).Select(e => e.Path).OrderBy(e => e);

        }

        private static IEnumerable<string> GetGroupFrameworks(IEnumerable<ContentItemGroup> groups)
        {
            return groups.Select(e => ((NuGetFramework)e.Properties["tfm"]).GetShortFolderName()).OrderBy(e => e);
        }
    }
}
