// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using Microsoft;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.RpcContracts.OutputChannel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.ServiceBroker;
using Microsoft.VisualStudio.Threading;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;

namespace IVsTestingExtension.Tests
{
    internal class OutputChannel : IDisposable
    {
        private static readonly Encoding TextEncoding = Encoding.UTF8;
        private static Guid guid = Guid.Parse("726822CA-DDC9-4213-9C7D-1C34A9BF2675");
        public static Guid guidVsWindowKindOutput = Guid.Parse("34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3");

        private readonly List<string> _deferredOutputMessages = new List<string>();
        private readonly AsyncSemaphore _pipeLock = new AsyncSemaphore(1);

        private readonly string _channelId;
        private readonly string _channelOutputName;
        private readonly JoinableTaskFactory _joinableTaskFactory;

        private AsyncLazy<ServiceBrokerClient> _serviceBrokerClient;
        private PipeWriter _channelPipeWriter;
        private bool _disposedValue = false;

        public OutputChannel(IAsyncServiceProvider asyncServiceProvider, JoinableTaskFactory joinableTaskFactory)
        {
            if (asyncServiceProvider == null)
            {
                throw new ArgumentNullException(nameof(asyncServiceProvider));
            }
            _channelId = guid.ToString();
            _channelOutputName = "Channel Store Test";
            _joinableTaskFactory = joinableTaskFactory ?? throw new ArgumentNullException(nameof(joinableTaskFactory));
            _serviceBrokerClient = new AsyncLazy<ServiceBrokerClient>(async () =>
            {
                IBrokeredServiceContainer container = (IBrokeredServiceContainer)await asyncServiceProvider.GetServiceAsync(typeof(SVsBrokeredServiceContainer));
                Assumes.Present(container);
                IServiceBroker sb = container.GetFullAccessServiceBroker();
                return new ServiceBrokerClient(sb, _joinableTaskFactory);
            }, _joinableTaskFactory);
        }

        public async Task ClearAsync()
        {
            await ClearThePaneAsync();
        }

        public void Write(string text)
        {
            _joinableTaskFactory.Run(() => SendOutputAsync(text, CancellationToken.None));
        }

        private async Task WriteToOutputChannelAsync(string channelId, string displayNameResourceId, string content, CancellationToken cancellationToken)
        {
            using (await _pipeLock.EnterAsync())
            {
                if (_channelPipeWriter == null)
                {
                    await OpenNewChannelAsync(channelId, displayNameResourceId, cancellationToken);

                    if (_channelPipeWriter == null)
                    {
                        // OutputChannel is not available so cache the output messages for later
                        _deferredOutputMessages.Add(content);
                        return;
                    }
                    else
                    {
                        // write any deferred messages
                        foreach (var s in _deferredOutputMessages)
                        {
                            // Flush when the original content is logged below
                            await _channelPipeWriter.WriteAsync(GetBytes(s), cancellationToken);
                        }
                        _deferredOutputMessages.Clear();
                    }
                }
                await _channelPipeWriter.WriteAsync(GetBytes(content), cancellationToken);
                await _channelPipeWriter.FlushAsync(cancellationToken);
            }
        }

        private async Task OpenNewChannelAsync(string channelId, string displayNameResourceId, CancellationToken cancellationToken)
        {
            using (var outputChannelStore = await (await _serviceBrokerClient.GetValueAsync()).GetProxyAsync<IOutputChannelStore>(VisualStudioServices.VS2019_4.OutputChannelStore, cancellationToken))
            {
                if (outputChannelStore.Proxy != null)
                {
                    var pipe = new Pipe();
                    await outputChannelStore.Proxy.CreateChannelAsync(channelId, displayNameResourceId, pipe.Reader, TextEncoding, cancellationToken);
                    _channelPipeWriter = pipe.Writer;
                }
            }
        }

        private static byte[] GetBytes(string content)
        {
            return TextEncoding.GetBytes(content);
        }

        private async Task SendOutputAsync(string message, CancellationToken cancellationToken)
        {
            await WriteToOutputChannelAsync(_channelId, _channelOutputName, message, cancellationToken);
        }

        private async Task CloseChannelAsync()
        {
            using (await _pipeLock.EnterAsync())
            {
                _channelPipeWriter?.CancelPendingFlush();
                _channelPipeWriter?.Complete();
                _channelPipeWriter = null;
            }
        }

        private async Task ClearThePaneAsync()
        {
            await CloseChannelAsync();
            await OpenNewChannelAsync(_channelId, _channelOutputName, CancellationToken.None);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_serviceBrokerClient.IsValueCreated)
                    {
                        _serviceBrokerClient.GetValue().Dispose();
                    }
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                    CloseChannelAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                }
                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
