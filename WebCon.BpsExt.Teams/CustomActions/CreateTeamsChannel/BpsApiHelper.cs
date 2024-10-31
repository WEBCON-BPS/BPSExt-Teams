using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebCon.BpsExt.Teams.CustomActions.Models;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Tools.Data;
using WebCon.WorkFlow.SDK.Tools.Data.Model;

namespace WebCon.BpsExt.Teams.CustomActions.CreateTeamsChannel
{
    class BpsApiHelper
    {
        private CreateTeamsChannelConfig _config;
        private ActionContextInfo _context;
        private StringBuilder _logger;

        public BpsApiHelper(CreateTeamsChannelConfig config, ActionContextInfo context, StringBuilder logger)
        {
            _config = config;
            _context = context;
            _logger = logger;
        }

        public async Task<List<ElementPrivileges>> GetWorkflofInstancePrivilegesAsync(int documentId)
        {
            var elementPrivilages = await GetElementPrivilegesAsync(_context.CurrentDbId, documentId);
            return GetDistincted(elementPrivilages.Privileges);
        }

        private async Task<string> GetAccessTokenAsync(WebServiceConnection connection)
        {       
            using (HttpClient client = new HttpClient())
            {
                var request = CreateAuthtRequest(connection);
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                    return await GetAccessTokenFromResponse(response);

                _logger.AppendLine($"An error occurred while downloading the access token. StatusCode: {response.StatusCode} Message: {await response.Content.ReadAsStringAsync()}");
                throw new Exception("An error occurred while downloading the access token. For more information check action logs");         
            }
        }

        private async Task<string> GetAccessTokenFromResponse(HttpResponseMessage response)
        {
            string result = await response.Content.ReadAsStringAsync();
            var authResponse = JsonConvert.DeserializeObject<AuthResponse>(result);
            return authResponse.AccessToken;
        }

        private HttpRequestMessage CreateAuthtRequest(WebServiceConnection connection)
        {
            var dict = new Dictionary<string, string>()
            {
                {"grant_type", "client_credentials"},
                {"client_id", connection.ClientID},
                {"client_secret", connection.ClientSecret}
            };
            return new HttpRequestMessage(HttpMethod.Post, $"{connection.Url}/api/oauth2/token") { Content = new FormUrlEncodedContent(dict) };
        }

        public async Task<PrivilegesList> GetElementPrivilegesAsync(int dbId, int elementId)
        {
            var connection = new ConnectionsHelper(_context).GetConnectionToWebService(new GetByConnectionParams(_config.ApiConfig.BpsApiConnectionId));
            var bearer = await GetAccessTokenAsync(connection);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
                var response = await client.GetAsync($"{connection.Url}/api/data/v5.0/db/{dbId}/elements/{elementId}/admin/privileges");

                if(response.IsSuccessStatusCode)
                    return await GetPrivilegesListAsync(response);

                _logger.AppendLine($"An error occurred while downloading privilages. StatusCode: {response.StatusCode} Message: {await response.Content.ReadAsStringAsync()}");
                throw new Exception($"An error occurred while downloading privilages for element {elementId}. For more information check action logs");
            }
        }

        private async Task<PrivilegesList> GetPrivilegesListAsync(HttpResponseMessage response)
        {
            var elementPrivilegesString = await response.Content.ReadAsStringAsync();
            var elementPrivilages = JsonConvert.DeserializeObject<PrivilegesList>(elementPrivilegesString);
            return elementPrivilages;
        }

        private List<ElementPrivileges> GetDistincted(List<ElementPrivileges> elemPrivilegesList)
        {
            var distinctedBpsIds =
             elemPrivilegesList
             .OrderBy(x => x.Level)
             .GroupBy(elemPrivileges => elemPrivileges.User.BpsId)
             .Select(g => g.First())
             .ToList();

            return distinctedBpsIds;
        }
    }
}
