using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace IVsTestingExtension.Tests
{
    [Export(typeof(ITestMethodProvider))]
    public class TestMethodProvider : ITestMethodProvider
    {
        public Func<Project, Dictionary<string, string>, Task> GetMethod() => WriteToOutputChannelAsync;

        private AsyncLazy<OutputChannel> _outputChannel = new AsyncLazy<OutputChannel>(async () => await InitializeOutputChannelAsync(), ThreadHelper.JoinableTaskFactory);
        
        private int iteration = 1;
        private async Task WriteToOutputChannelAsync(Project projectSelected, Dictionary<string, string> arguments)
        {
            var channel = await _outputChannel.GetValueAsync();

            channel.Clear();
            await Task.Delay(20);
            channel.Write(iteration + ": Starting restore now..." + Environment.NewLine);
            await Task.Delay(20);
            channel.Write(iteration + ": Restored A" + Environment.NewLine);
            await Task.Delay(20);
            channel.Write(iteration + ": Restored B" + Environment.NewLine);
            await Task.Delay(20);
            channel.Write(iteration + ": Restored C" + Environment.NewLine);
            await Task.Delay(20);
            channel.Write(iteration + ": Done!" + Environment.NewLine);
            await Task.Delay(20);
            iteration++;
        }

        private static Task<OutputChannel> InitializeOutputChannelAsync()
        {
            var asyncServiceProvider = AsyncServiceProvider.GlobalProvider;
            return Task.FromResult(new OutputChannel(asyncServiceProvider, ThreadHelper.JoinableTaskFactory));
        }
    }
}
