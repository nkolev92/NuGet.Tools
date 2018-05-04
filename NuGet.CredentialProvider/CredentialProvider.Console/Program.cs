using CredentialProvider.RequestHandlers;
using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CredentialProvider
{
    public static class Program
    {
        public static readonly TraceSource Logger = new TraceSource("CredentialPlugin");

        public static async Task<int> Main(string[] args)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                tokenSource.Cancel();
                eventArgs.Cancel = true;
            };


            IRequestHandlers requestHandlers = new RequestHandlerCollection
            {
                { MessageMethod.GetAuthenticationCredentials, new GetAuthenticationCredentialsRequestHandler(Logger) },
                { MessageMethod.GetOperationClaims, new GetOperationClaimsRequestHandler(Logger) },
                { MessageMethod.Initialize, new InitializeRequestHandler(Logger) },
            };

            if (String.Equals(args.SingleOrDefault(), "plugin", StringComparison.OrdinalIgnoreCase))
            {
                using (IPlugin plugin = await PluginFactory.CreateFromCurrentProcessAsync(requestHandlers, ConnectionOptions.CreateDefault(), tokenSource.Token).ConfigureAwait(continueOnCapturedContext: false))
                {
                    await RunNuGetPluginsAsync(plugin, Logger, tokenSource.Token).ConfigureAwait(continueOnCapturedContext: false);
                }

                return 0;
            }

            if (requestHandlers.TryGet(MessageMethod.GetAuthenticationCredentials, out IRequestHandler requestHandler) && requestHandler is GetAuthenticationCredentialsRequestHandler getAuthenticationCredentialsRequestHandler)
            {
                GetAuthenticationCredentialsRequest request = new GetAuthenticationCredentialsRequest(new Uri(args[0]), isRetry: false, nonInteractive: true);

                GetAuthenticationCredentialsResponse response = await getAuthenticationCredentialsRequestHandler.HandleRequestAsync(request).ConfigureAwait(continueOnCapturedContext: false);

                Console.WriteLine(response?.Username);
                Console.WriteLine(response?.Password);
                Console.WriteLine(response?.Password.ToJsonWebTokenString());

                return 0;
            }

            return -1;
        }

        internal static async Task RunNuGetPluginsAsync(IPlugin plugin, TraceSource traceSource, CancellationToken cancellationToken)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0);

            plugin.Connection.Faulted += (sender, a) =>
            {
                traceSource.Error($"Faulted on message: {a.Message?.Type} {a.Message?.Method} {a.Message?.RequestId}");
                traceSource.Error(a.Exception.ToString());
            };

            plugin.Closed += (sender, a) => semaphore.Release();

            bool complete = await semaphore.WaitAsync(TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            if (!complete)
            {
                Logger.Error("Timed out waiting for plug-in operations to complete");
            }
        }
    }
}