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
        public static void AnalyzePackageDependenciesCompatibility(this PackageCompatibilityInfo analyzedPackage, NuspecReader nuspecReader)
        {
            if (nuspecReader != null)
            {
                analyzedPackage.DependencyGroupFrameworks = nuspecReader.GetDependencyGroups().Select(e => e.TargetFramework.GetShortFolderName());
            }
        }

        public static void AnalyzePackageContentCompatibility(this PackageCompatibilityInfo analyzedPackage, IEnumerable<string> fileList)
        {
            if (fileList != null)
            {
                var managedCodeConventions = new ManagedCodeConventions(new NuGet.RuntimeModel.RuntimeGraph());
                var collection = new ContentItemCollection();
                collection.Load(fileList);

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
            return groups.Select(e => ((NuGetFramework)e.Properties["tfm"]).GetShortFolderName()).OrderBy(e => e);
        }

        private static IEnumerable<string> GetGroupFrameworks(IEnumerable<ContentItemGroup> groups)
        {
            return groups.SelectMany(e => e.Items).Select(e => e.Path).OrderBy(e => e);
        }
    }
}
