using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CredentialProvider.RequestHandlers
{
    internal class GetAuthenticationCredentialsRequestHandler : RequestHandlerBase<GetAuthenticationCredentialsRequest, GetAuthenticationCredentialsResponse>
    {

        public GetAuthenticationCredentialsRequestHandler(TraceSource logger)
            : base(logger)
        {
        }

        public override Task<GetAuthenticationCredentialsResponse> HandleRequestAsync(GetAuthenticationCredentialsRequest request)
        {
            return Task.FromResult(new GetAuthenticationCredentialsResponse(
                username: "username",
                password: "password",
                message: null,
                authenticationTypes: new List<string> { "basic" },
                responseCode: MessageResponseCode.Success));
        }
    }
}