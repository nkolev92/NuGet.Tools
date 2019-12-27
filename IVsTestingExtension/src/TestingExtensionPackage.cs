using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using IVsTestingExtension.ToolWindows;
using Microsoft;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio;
using Task = System.Threading.Tasks.Task;

namespace IVsTestingExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("IVs Testing Extension", "Helps test the IVs APIs in Visual Studio by invoking on different threading contexts.", "1.0")]
    [ProvideToolWindow(typeof(CommandInvokingWindow), Style = VsDockStyle.Tabbed, DockedWidth = 300, Window = "DocumentWell", Orientation = ToolWindowOrientation.Left)]
    [Guid("5A86D30C-B4EC-4537-8B81-A422058075CE")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class TestingExtensionPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var componentModel = await this.GetComponentModelAsync();
            componentModel.DefaultCompositionService.SatisfyImportsOnce(this);

            await JoinableTaskFactory.SwitchToMainThreadAsync();
            await ShowToolWindow.InitializeAsync(this);
        }

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            return toolWindowType.Equals(Guid.Parse(CommandInvokingWindow.WindowGuidString)) ? this : null;
        }

        protected override string GetToolWindowTitle(Type toolWindowType, int id)
        {
            return toolWindowType == typeof(CommandInvokingWindow) ? CommandInvokingWindow.Title : base.GetToolWindowTitle(toolWindowType, id);
        }

        internal const string IVsAsyncPackageInstallerServiceName = "IVsAsyncPackageInstaller";
        internal const string IVsAsyncPackageInstallerServiceVersion = "1.0";

        internal static ServiceRpcDescriptor Descriptor { get; } = new ServiceJsonRpcDescriptor(
            new ServiceMoniker(IVsAsyncPackageInstallerServiceName, new Version(IVsAsyncPackageInstallerServiceVersion)),
            ServiceJsonRpcDescriptor.Formatters.MessagePack,
            ServiceJsonRpcDescriptor.MessageDelimiters.BigEndianInt32LengthHeader);

        protected override async Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
            var dte = await this.GetDTEAsync();
            IVsAsyncPackageInstaller client = await GetIVsPackageInstallerClientAsync();

            Task testMethodAsync(string project, Dictionary<string, string> arguments) => TestMethodAsync(client, project, arguments);
            var model = new ProjectCommandTestingModel(dte, testMethodAsync);
            await model.InitializeAsync();
            return model;
        }

        private async Task<IVsAsyncPackageInstaller> GetIVsPackageInstallerClientAsync()
        {
            IBrokeredServiceContainer brokeredServiceContainer = await this.GetServiceAsync<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
            Assumes.Present(brokeredServiceContainer);
            IServiceBroker sb = brokeredServiceContainer.GetFullAccessServiceBroker();
            IVsAsyncPackageInstaller client = await sb.GetProxyAsync<IVsAsyncPackageInstaller>(Descriptor, this.DisposalToken);
            return client;
        }

        private async Task TestMethodAsync(IVsAsyncPackageInstaller VsAsyncPackageInstaller, string projectSelected, Dictionary<string, string> arguments)
        {
            IVsAsyncPackageInstaller client2 = await GetIVsPackageInstallerClientAsync();

            arguments.TryGetValue("packageId", out string packageId);
            arguments.TryGetValue("packageVersion", out string packageVersion);
            arguments.TryGetValue("source", out string source);
            arguments.TryGetValue("ignoreDependencies", out string ignoreDependenciesStr);
            bool.TryParse(ignoreDependenciesStr, out bool ignoreDependencies);

            await VsAsyncPackageInstaller.InstallPackageAsync(source: source,
                                              projectSelected,
                                              packageId,
                                              packageVersion,
                                              ignoreDependencies: ignoreDependencies);
        }
    }
}
