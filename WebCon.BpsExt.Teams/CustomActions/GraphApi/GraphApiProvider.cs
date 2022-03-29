using Microsoft.Graph;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

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
            var connection = WebCon.WorkFlow.SDK.Tools.Data.ConnectionsHelper.GetConnectionToWebService(new WorkFlow.SDK.Tools.Data.Model.GetByConnectionParams(_connectionId, _context));      
            _logger.AppendLine("Creating graph client");
            string[] scopes = new string[] { ".default" };
            var authProvider = new AuthenticationProvider(connection.ClientID, connection.ClientSecret, scopes, connection.AuthorizationServiceUrl);
            return new GraphServiceClient("https://graph.microsoft.com/v1.0", authProvider);
        }     
    }
}
