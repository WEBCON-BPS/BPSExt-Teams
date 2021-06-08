using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BPSExt.Teams.CustomActions.GraphApi;
using BPSExt.Teams.CustomActions.Models;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace BPSExt.Teams.CustomActions.CreateTeamsChannel
{
    public class GraphApiTeamHelper : GraphApiProvider
    {
        private CreateTeamsChannelConfig _config;

        public GraphApiTeamHelper(CreateTeamsChannelConfig teamConfig, ActionContextInfo context, StringBuilder log) : base(teamConfig.ApiConfig.GraphApiConnectionId, log, context)
        {
            _config = teamConfig;
        }

        public DataToSave CreateTeamsChannel(List<ElementPrivileges> users)
        {
            var graphClient = CreateGraphClient();
            var (members, owner) = GetAllMembers(graphClient, users);
            var team = CreateTeam(graphClient, owner);
            var channel = CreateChannel(graphClient, team);
            AddMembers(graphClient, team.Id, members);
            return new DataToSave()
            {
                ChannelId = channel.Id,
                ChannelWebUrl = channel.WebUrl,
                TeamId = team.Id
            };
        }

        private (List<string> members, string owner) GetAllMembers(GraphServiceClient graphClient, List<ElementPrivileges> users)
        {
            _logger.AppendLine("Getting group members");
            var members = GetMembers(graphClient, users);
            var owner = GetAsUser(graphClient, _config.TeamsConfig.TeamOwner);
            members = members.Where(x => !x.Equals(owner, StringComparison.InvariantCultureIgnoreCase)).ToList();
            return (members, owner);
        }

        private List<string> GetMembers(GraphServiceClient graphClient, List<ElementPrivileges> users)
        {
            var members = new List<string>();
            foreach (var user in users)
            {
                var userInfo = WebCon.WorkFlow.SDK.Tools.Users.UserDataProvider.Validate(WebCon.WorkFlow.SDK.Tools.Users.Model.SearchParameters.FromContent(user.User.BpsId));
                if (userInfo != null)
                    if (userInfo.AccountType == WebCon.WorkFlow.SDK.Common.Model.BpsAccountType.User)
                        members.Add(GetAsUser(graphClient, user.User.BpsId));
                    else if (userInfo.AccountType == WebCon.WorkFlow.SDK.Common.Model.BpsAccountType.ADGroup)
                        members.AddRange(GetAsGroup(graphClient, user));
            }
            return members;
        }

        private List<string> GetAsGroup(GraphServiceClient graphClient, ElementPrivileges groupToAdd)
        {
            var members = GetUsersFromGroup(graphClient, groupToAdd);
            return members.Select(x => x.Id).ToList();
        }

        private string GetAsUser(GraphServiceClient graphClient, string user)
        {
            return graphClient.Users[user].Request().GetAsync().Result.Id;
        }

        private List<Microsoft.Graph.User> GetUsersFromGroup(GraphServiceClient graphClient, ElementPrivileges group)
        {
            var groupToAdd = graphClient.Groups
             .Request().Filter($"startswith(displayName, '{group.User.Name}')")
             .GetAsync().Result.FirstOrDefault();

            var members = graphClient.Groups[groupToAdd.Id].TransitiveMembers
            .Request()
            .GetAsync().Result;

            var users = members.Where(x => x is Microsoft.Graph.User).Select(x => (Microsoft.Graph.User)x).ToList();
            return users.Where(x => x.UserPrincipalName != _config.TeamsConfig.TeamOwner).Select(x => x).ToList();
        }

        private Channel CreateChannel(GraphServiceClient graphClient, Team team)
        {
            _logger.AppendLine("Creating channel");
            var channel = new Channel
            {
                DisplayName = _config.TeamsConfig.ChannelName,
                Description = _config.TeamsConfig.TeamDescription
            };

            var channelResult = graphClient.Teams[team.Id].Channels
               .Request()
               .AddAsync(channel).Result;

            return channelResult;
        }

        private Team CreateTeam(GraphServiceClient graphClient, string owner)
        {
            _logger.AppendLine("Creatinng team");
            var team = new Team
            {
                DisplayName = _config.TeamsConfig.TeamName,
                Description = _config.TeamsConfig.TeamDescription,
                Members = new TeamMembersCollectionPage()
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

            var createdTeam = graphClient.Teams//returns null...
               .Request()
               .AddAsync(team).Result;

            return GetTeamToReturn(graphClient, owner, 0);//so we have to download it
        }


        private Team GetTeamToReturn(GraphServiceClient graphClient, string owner, int counter)
        {
            _logger.AppendLine($"Downloadnig team id. Attempt: {counter}");
            Thread.Sleep(5000);
            var joinedTeams = graphClient.Users[owner].JoinedTeams.Request().GetAsync().Result;
            var teamToReturn = joinedTeams.Where(x => x.DisplayName == _config.TeamsConfig.TeamName).FirstOrDefault();

            if (teamToReturn == null && counter < 6)
                teamToReturn = GetTeamToReturn(graphClient, owner, ++counter);

            return teamToReturn;
        }

        private void AddMembers(GraphServiceClient graphClient, string teamId, List<string> membersIds)
        {
            foreach (var member in membersIds)
                AddMember(graphClient, teamId, member, "member");
        }

        private void AddMember(GraphServiceClient graphClient, string teamId, string id, string role)
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
            var addedMember = graphClient.Teams[teamId].Members
                .Request()
                .AddAsync(conversationMember).Result;
        }
    }
}
