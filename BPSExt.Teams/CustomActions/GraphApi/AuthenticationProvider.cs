using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Threading.Tasks;

namespace BPSExt.Teams.CustomActions.GraphApi
{
    public class AuthenticationProvider : IAuthenticationProvider
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string[] appScopes;
        private readonly string tenantId;

        public AuthenticationProvider(string clientId, string clientSecret, string[] appScopes, string tenantId)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.appScopes = appScopes;
            this.tenantId = tenantId;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var clientApplication = ConfidentialClientApplicationBuilder.Create(this.clientId)
                .WithClientSecret(this.clientSecret)
                .WithClientId(this.clientId)
                .WithTenantId(this.tenantId)
                .Build();

            var result = await clientApplication.AcquireTokenForClient(this.appScopes).ExecuteAsync();

            request.Headers.Add("Authorization", result.CreateAuthorizationHeader());
        }
    }
}
