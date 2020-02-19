using EnvDTE;
using Microsoft;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.RpcContracts.OutputChannel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using NuGet.VisualStudio;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace IVsTestingExtension.Tests
{
    [Export(typeof(ITestMethodProvider))]
    public class TestMethodProvider : ITestMethodProvider
    {
        [Import]
        IVsPackageInstaller VsAsyncPackageInstaller { get; set; }

        public Func<Project, Dictionary<string, string>, Task> GetMethod() => TestSyncInstallPackage;

        private async Task TestSyncInstallPackage(Project projectSelected, Dictionary<string, string> arguments)
        {
            var service = await AcquireServiceAsync(CancellationToken.None, AsyncServiceProvider.GlobalProvider);
            service.WriteLineAsync("myChannel", "We are writing a message!", CancellationToken.None);
            //arguments.TryGetValue("packageId", out string packageId);
            //arguments.TryGetValue("packageVersion", out string packageVersion);
            //arguments.TryGetValue("source", out string source);
            //arguments.TryGetValue("ignoreDependencies", out string ignoreDependenciesStr);
            //bool.TryParse(ignoreDependenciesStr, out bool ignoreDependencies);

            //VsAsyncPackageInstaller.InstallPackage(source: source,
            //                                  projectSelected,
            //                                  packageId,
            //                                  packageVersion,
            //                                  ignoreDependencies: ignoreDependencies);
        }

        private async Task<IOutputChannelStore> AcquireServiceAsync(CancellationToken cancellationToken, IAsyncServiceProvider asyncServiceProvider)
        {
            ServiceActivationOptions options = new ServiceActivationOptions();
            options.SetClientDefaults();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IBrokeredServiceContainer brokeredServiceContainer = await asyncServiceProvider.GetServiceAsync<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
            Assumes.Present(brokeredServiceContainer);
            IServiceBroker sb = brokeredServiceContainer.GetFullAccessServiceBroker();
            return await sb.GetProxyAsync<IOutputChannelStore>(VisualStudioServices.VS2019_4.OutputChannelStore, options, cancellationToken);
        }
    }
}
