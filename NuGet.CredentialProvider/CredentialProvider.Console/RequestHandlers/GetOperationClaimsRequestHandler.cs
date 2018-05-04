using NuGet.Protocol.Plugins;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CredentialProvider.RequestHandlers
{
    internal class GetOperationClaimsRequestHandler : RequestHandlerBase<GetOperationClaimsRequest, GetOperationClaimsResponse>
    {
        public GetOperationClaimsRequestHandler(TraceSource logger)
            : base(logger)
        {
        }

        public override Task<GetOperationClaimsResponse> HandleRequestAsync(GetOperationClaimsRequest request)
        {
            var operationClaims = new List<OperationClaim>();

            if (request.PackageSourceRepository == null && request.ServiceIndex == null)
            {
                operationClaims.Add(OperationClaim.Authentication);
            }

            return Task.FromResult(new GetOperationClaimsResponse(operationClaims));
        }
    }
}