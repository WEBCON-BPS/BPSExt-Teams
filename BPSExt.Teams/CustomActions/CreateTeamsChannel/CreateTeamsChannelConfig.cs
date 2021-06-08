using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace BPSExt.Teams.CustomActions.CreateTeamsChannel
{
    public class CreateTeamsChannelConfig : PluginConfiguration
    {

        [ConfigGroupBox(DisplayName = "Api Configuration")]
        public ApiConfiguration ApiConfig { get; set; }

        [ConfigGroupBox(DisplayName = "Teams Configuration")]
        public TeamsConfiguration TeamsConfig { get; set; }

        [ConfigGroupBox(DisplayName = "Additional Configuration", Description = "Created channel information for later use")]
        public AdditionalConfiguration AdditionalConfig { get; set; }
    }

    public class TeamsConfiguration
    {
        [ConfigEditableText(DisplayName = "Team name", IsRequired = true)]
        public string TeamName { get; set; }

        [ConfigEditableText(DisplayName = "Team description", IsRequired = true)]
        public string TeamDescription { get; set; }

        [ConfigEditableText(DisplayName = "Channel name", IsRequired = true, MaxLength = 50, Description = "Max length is 50 characters")]
        public string ChannelName { get; set; }

        [ConfigEditableText(DisplayName = "Team owner", IsRequired = true, Description = "Team owner login in upn format")]
        public string TeamOwner { get; set; }
    }

    public class AdditionalConfiguration
    {
        [ConfigEditableFormFieldID(DisplayName = "Field for channel WebUrl")]
        public int ChannelUrlFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Field for channel Id")]
        public int ChannelIdFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Field for team Id")]
        public int TeamIdFieldId { get; set; }
    }

    public class ApiConfiguration
    {
        [ConfigEditableConnectionID(DisplayName = "Connection to GraphAPI", IsRequired = true, ConnectionsType = DataConnectionType.WebServiceREST)]
        public int GraphApiConnectionId { get; set; }

        [ConfigEditableConnectionID(DisplayName = "Connection to BPS API", IsRequired = true, ConnectionsType = DataConnectionType.WebServiceREST)]
        public int BpsApiConnectionId { get; set; }
    }
}