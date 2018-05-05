using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CredentialProvider.RequestHandlers
{
    internal class GetAuthenticationCredentialsRequestHandler : RequestHandlerBase<GetAuthenticationCredentialsRequest, GetAuthenticationCredentialsResponse>
    {
        private PluginToClientRequestHandler _requestHandler;
        public GetAuthenticationCredentialsRequestHandler(TraceSource logger, PluginToClientRequestHandler requestHandler)
            : base(logger)
        {
            _requestHandler = requestHandler;
        }

        private async Task SendLogMessage()
        {
            var payload = new LogRequest(NuGet.Common.LogLevel.Minimal, message: "Please go to bla bla to authenticate");
            var request = MessageUtilities.Create(
                requestId: "b",
                type: MessageType.Request,
                method: MessageMethod.Log,
                payload: payload);

            await _requestHandler.SendRequest(request, CancellationToken.None);
        }

        public override async Task<GetAuthenticationCredentialsResponse> HandleRequestAsync(GetAuthenticationCredentialsRequest request)
        {
            await SendLogMessage();
            await Task.Delay(10000);

            return new GetAuthenticationCredentialsResponse(
                username: "username",
                password: "password",
                message: null,
                authenticationTypes: new List<string> { "basic" },
                responseCode: MessageResponseCode.Success);
        }
    }
}