using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using BPSExt.Teams.CustomActions.Models;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Tools.Data.Model;

namespace BPSExt.Teams.CustomActions.CreateTeamsChannel
{
    class BpsApiHelper
    {
        private CreateTeamsChannelConfig _config;
        private ActionContextInfo _context;

        public BpsApiHelper(CreateTeamsChannelConfig config, ActionContextInfo context)
        {
            _config = config;
            _context = context;
        }

        public List<ElementPrivileges> GetWorkflofInstancePrivileges(int documentId)
        {
            var elementPrivilages = GetElementPrivileges(1, documentId).Privileges;
            return GetDistincted(elementPrivilages);
        }

        private token GetToken(WebServiceConnection connection)
        {       
            LoginModel loginModel = new LoginModel();
            loginModel.clientId = connection.ClientID;
            loginModel.clientSecret = connection.ClientSecret;

            string json = JsonConvert.SerializeObject(loginModel);

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                var result = client.PostAsync($"{connection.Url}/api/login", content).Result.Content.ReadAsStringAsync();

                string TokenString = result.Result;
                token token = new token();
                token = JsonConvert.DeserializeObject<token>(TokenString);
                return token;
            }
        }

        public PrivilegesList GetElementPrivileges(int dbId, int elementId)
        {
            var connection = WebCon.WorkFlow.SDK.Tools.Data.ConnectionsHelper.GetConnectionToWebService(new GetByConnectionParams(_config.ApiConfig.BpsApiConnectionId, _context));
            var bearer = GetToken(connection);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                  new AuthenticationHeaderValue("Bearer", bearer.Token);

                var response = client.GetAsync($"{connection.Url}/api/data/v3.0/db/{dbId}/elements/{elementId}/admin/privileges").Result.Content.ReadAsStringAsync();
                var elementPrivilegesString = response.Result;

                var elementPrivilages = JsonConvert.DeserializeObject<PrivilegesList>(elementPrivilegesString);
                return elementPrivilages;
            }
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
