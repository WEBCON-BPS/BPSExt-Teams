using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebCon.BpsExt.Teams.CustomActions.GraphApi;
using WebCon.BpsExt.Teams.CustomActions.Models;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Common.Model;
using WebCon.WorkFlow.SDK.Tools.Users;
using WebCon.WorkFlow.SDK.Tools.Users.Model;

namespace WebCon.BpsExt.Teams.CustomActions.CreateTeamsChannel
{
    public class GraphApiTeamHelper : GraphApiProvider
    {
        private CreateTeamsChannelConfig _config;

        public GraphApiTeamHelper(CreateTeamsChannelConfig teamConfig, ActionContextInfo context, StringBuilder log) : base(teamConfig.ApiConfig.GraphApiConnectionId, log, context)
        {
            _config = teamConfig;
        }

        public async Task<DataToSave> CreateTeamsChannelAsync(List<ElementPrivileges> users)
        {
            var graphClient = CreateGraphClient(_config.ApiConfig.UseProxy);
            var (members, owner) = await GetAllMembersAsync(graphClient, users);
            var team = await CreateTeamAsync(graphClient, owner);
            var channel = await CreateChannelAsync(graphClient, team);
            await AddMembersAsync(graphClient, team.Id, members);
            return new DataToSave()
            {
                ChannelId = channel.Id,
                ChannelWebUrl = channel.WebUrl,
                TeamId = team.Id
            };
        }

        private async Task<(List<string> members, string owner)> GetAllMembersAsync(GraphServiceClient graphClient, List<ElementPrivileges> users)
        {
            _logger.AppendLine("Getting group members");
            var members = await GetMembersAsync(graphClient, users);
            var owner = await GetAsUserAsync(graphClient, _config.TeamsConfig.TeamOwner);
            members = members.Where(x => !x.Equals(owner, StringComparison.InvariantCultureIgnoreCase)).ToList();
            return (members, owner);
        }

        private async Task<List<string>> GetMembersAsync(GraphServiceClient graphClient, List<ElementPrivileges> users)
        {
            var members = new List<string>();
            var userDataProvider = new UserDataProvider(base._context);
            foreach (var user in users)
            {
                var userInfo = await userDataProvider.ValidateAsync(SearchParameters.FromContent(user.User.BpsId));
                if (userInfo != null)
                    if (userInfo.AccountType == BpsAccountType.User)
                        members.Add(await GetAsUserAsync(graphClient, user.User.BpsId));
                    else if (userInfo.AccountType == BpsAccountType.ADGroup)
                        members.AddRange(await GetAsGroupAsync(graphClient, user));
            }
            return members;
        }

        private async Task<List<string>> GetAsGroupAsync(GraphServiceClient graphClient, ElementPrivileges groupToAdd)
        {
            var members = await GetUsersFromGroupAsync(graphClient, groupToAdd);
            return members.Select(x => x.Id).ToList();
        }

        private async Task<string> GetAsUserAsync(GraphServiceClient graphClient, string user)
        {
            return (await graphClient.Users[user].GetAsync()).Id;
        }

        private async Task<List<Microsoft.Graph.Models.User>> GetUsersFromGroupAsync(GraphServiceClient graphClient, ElementPrivileges group)
        {
            var groupToAdd = (await graphClient.Groups.GetAsync(config =>
            {
                config.QueryParameters.Filter = $"startswith(displayName, '{group.User.Name}')";
            })).Value.FirstOrDefault();

            var members = await graphClient.Groups[groupToAdd.Id].TransitiveMembers.GetAsync();

            var users = members.Value.Where(x => x is Microsoft.Graph.Models.User).Select(x => (Microsoft.Graph.Models.User)x).ToList();
            return users.Where(x => x.UserPrincipalName != _config.TeamsConfig.TeamOwner).Select(x => x).ToList();
        }

        private async Task<Channel> CreateChannelAsync(GraphServiceClient graphClient, Team team)
        {
            _logger.AppendLine("Creating channel");
            var channel = new Channel
            {
                DisplayName = _config.TeamsConfig.ChannelName,
                Description = _config.TeamsConfig.TeamDescription
            };

            var channelResult = await graphClient.Teams[team.Id].Channels.PostAsync(channel);

            return channelResult;
        }

        private async Task<Team> CreateTeamAsync(GraphServiceClient graphClient, string owner)
        {
            _logger.AppendLine("Creatinng team");
            var team = new Team
            {
                DisplayName = _config.TeamsConfig.TeamName,
                Description = _config.TeamsConfig.TeamDescription,
                Members = new List<ConversationMember>()
                {
                    new AadUserConversationMember
                    {
                        Roles = new List<String>()
                        {
                            "owner"
                        },
                        AdditionalData = new Dictionary<string, object>()
                        {
                            {"user@odata.bind", $"https://graph.microsoft.com/v1.0/users('{owner}')"}
                        }
                    }
                },
                AdditionalData = new Dictionary<string, object>()
                {
                    {"template@odata.bind", "https://graph.microsoft.com/v1.0/teamsTemplates('standard')"}
                }
            };

            var createdTeam = await graphClient.Teams
               .PostAsync(team);//returns null...

            return await GetTeamToReturnAsync(graphClient, owner, 0);//so we have to download it
        }


        private async Task<Team> GetTeamToReturnAsync(GraphServiceClient graphClient, string owner, int counter)
        {
            _logger.AppendLine($"Downloadnig team id. Attempt: {counter}");
            Thread.Sleep(5000);
            var joinedTeams = await graphClient.Users[owner].JoinedTeams.GetAsync();
            var teamToReturn = joinedTeams.Value.Where(x => x.DisplayName == _config.TeamsConfig.TeamName).FirstOrDefault();

            if (teamToReturn == null && counter < 6)
                teamToReturn = await GetTeamToReturnAsync(graphClient, owner, ++counter);

            return teamToReturn;
        }

        private async Task AddMembersAsync(GraphServiceClient graphClient, string teamId, List<string> membersIds)
        {
            foreach (var member in membersIds)
                await AddMemberAsync(graphClient, teamId, member, "member");
        }

        private async Task AddMemberAsync(GraphServiceClient graphClient, string teamId, string id, string role)
        {
            var conversationMember = new AadUserConversationMember
            {
                Roles = new List<String>()
                {
                    role
                },
                AdditionalData = new Dictionary<string, object>()
                {
                    {"user@odata.bind", $"https://graph.microsoft.com/v1.0/users('{id}')"}
                }
            };
            var addedMember = await graphClient.Teams[teamId].Members.PostAsync(conversationMember);
        }
    }
}
