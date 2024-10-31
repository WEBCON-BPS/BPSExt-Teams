using Azure.Identity;
using Microsoft.Graph;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Tools.Data;
using WebCon.WorkFlow.SDK.Tools.Data.Model;

namespace WebCon.BpsExt.Teams.CustomActions.GraphApi
{
    public class GraphApiProvider
    {
        private int _connectionId;
        internal StringBuilder _logger;
        internal ActionContextInfo _context;

        public GraphApiProvider(int connectionId, StringBuilder log, ActionContextInfo context)
        {
            _connectionId = connectionId;
            _logger = log;
            _context = context;
        }

        internal GraphServiceClient CreateGraphClient()
        {
            var connection = new ConnectionsHelper(_context).GetConnectionToWebService(new GetByConnectionParams(_connectionId));
            _logger.AppendLine("Creating graph client");

            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(connection.AuthorizationServiceUrl, connection.ClientID, connection.ClientSecret, options);
            return new GraphServiceClient(clientSecretCredential, new[] { "https://graph.microsoft.com/.default" });
        }     
    }
}
