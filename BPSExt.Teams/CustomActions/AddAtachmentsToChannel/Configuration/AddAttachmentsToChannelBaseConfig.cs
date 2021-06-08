using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace BPSExt.Teams.CustomActions.AddAttachmentsToChannel.Configuration
{
    public class AddAttachmentsToChannelBaseConfig : PluginConfiguration
    {
        [ConfigEditableConnectionID(DisplayName = "Connection to GraphAPI", IsRequired = true, ConnectionsType = DataConnectionType.WebServiceREST)]
        public int ConnectionId { get; set; }

        [ConfigEditableText(DisplayName = "Sql query returning attachments", IsRequired = true, Multiline = true, TagEvaluationMode = EvaluationMode.SQL, 
            Description = "Sql query returning the id of attachments to be uploaded to the channel")]
        public string AttachmentsQuery { get; set; }

        [ConfigEditableText(DisplayName = "Channel name", IsRequired = true)]
        public string ChannelName { get; set; }

        [ConfigEditableText(DisplayName = "Team id", IsRequired = true)]
        public string TeamId { get; set; }

        [ConfigEditableText(DisplayName = "Team name")]
        public string TeamName { get; set; }
    }
}