using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Teams.CustomActions.AddAtachmentsToChannel
{
    public class AddAttachmentToChannelConfig : PluginConfiguration
    {
        [ConfigEditableConnectionID(DisplayName = "Connection to GraphAPI", IsRequired = true, ConnectionsType = DataConnectionType.WebServiceREST)]
        public int ConnectionId { get; set; }

        [ConfigEditableBool("Use proxy")]
        public bool UseProxy { get; set; }

        [ConfigEditableText(DisplayName = "Sql query returning attachments", IsRequired = true, Multiline = true, TagEvaluationMode = EvaluationMode.SQL,
            Description = "Sql query returning the id of attachments to be uploaded to the channel")]
        public string AttachmentsQuery { get; set; }

        [ConfigEditableText(DisplayName = "Channel id", IsRequired = true)]
        public string ChannelId { get; set; }

        [ConfigEditableText(DisplayName = "Team id", IsRequired = true)]
        public string TeamId { get; set; }
    }
}