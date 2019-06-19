using System;
using System.Collections.Generic;
using System.Linq;

namespace PackageExplorer
{
    public class PackageCompatibilityInfo
    {
        public string Id { get; }
        public string Version { get; }

        public IEnumerable<string> AllFiles { get; set; }

        public IEnumerable<string> DependencyGroupFrameworks { get; set; }

        public IEnumerable<string> LibFrameworks { get; set; }
        public IEnumerable<string> LibFiles { get; set; }


        public IEnumerable<string> RefFrameworks { get; set; }
        public IEnumerable<string> RefFiles { get; set; }


        public IEnumerable<string> RuntimeFrameworks { get; set; }
        public IEnumerable<string> RuntimeFiles { get; set; }


        public IEnumerable<string> MSBuildMultiTargetingFrameworks { get; set; }
        public IEnumerable<string> MSBuildMultiTargetingFiles { get; set; }


        public IEnumerable<string> MSBuildFrameworks { get; set; }
        public IEnumerable<string> MSBuildFiles { get; set; }


        public IEnumerable<string> MSBuildTransitiveFrameworks { get; set; }
        public IEnumerable<string> MSBuildTransitiveFiles { get; set; }


        public IEnumerable<string> AnyTargetFrameworks { get; set; }
        public IEnumerable<string> AnyTargetFiles { get; set; }


        public PackageCompatibilityInfo(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public override string ToString()
        {
            return $"Id={Id}" + Environment.NewLine +
                   $"Version={Version}" + Environment.NewLine +
                   $"AllFiles={string.Join(",", AllFiles.ToArray())}" + Environment.NewLine +
                   $"DependencyGroupFrameworks={string.Join(",", DependencyGroupFrameworks.ToArray())}" + Environment.NewLine +
                   $"LibFrameworks={string.Join(",", LibFrameworks.ToArray())}" + Environment.NewLine +
                   $"LibFiles={string.Join(",", LibFiles.ToArray())}" + Environment.NewLine +
                   $"RefFrameworks={string.Join(",", RefFrameworks.ToArray())}" + Environment.NewLine +
                   $"RefFiles={string.Join(",", RefFiles.ToArray())}" + Environment.NewLine +
                   $"RuntimeFrameworks={string.Join(",", RuntimeFrameworks.ToArray())}" + Environment.NewLine +
                   $"RuntimeFiles={string.Join(",", RuntimeFiles.ToArray())}" + Environment.NewLine +
                   $"MSBuildMultiTargetingFrameworks={string.Join(",", MSBuildMultiTargetingFrameworks.ToArray())}" + Environment.NewLine +
                   $"MSBuildMultiTargetingFiles={string.Join(",", MSBuildMultiTargetingFiles.ToArray())}" + Environment.NewLine +
                   $"MSBuildFrameworks={string.Join(",", MSBuildFrameworks.ToArray())}" + Environment.NewLine +
                   $"MSBuildFiles={string.Join(",", MSBuildFiles.ToArray())}" + Environment.NewLine +
                   $"MSBuildTransitiveFrameworks={string.Join(",", MSBuildTransitiveFrameworks.ToArray())}" + Environment.NewLine +
                   $"MSBuildTransitiveFiles={string.Join(",", MSBuildTransitiveFiles.ToArray())}" + Environment.NewLine +
                   $"AnyTargetFrameworks={string.Join(",", AnyTargetFrameworks.ToArray())}" + Environment.NewLine +
                   $"AnyTargetFiles={string.Join(",", AnyTargetFiles.ToArray())}";

        }
    }
}
