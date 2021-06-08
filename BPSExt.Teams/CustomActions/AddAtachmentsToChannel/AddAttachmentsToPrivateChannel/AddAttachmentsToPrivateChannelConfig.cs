using BPSExt.Teams.CustomActions.AddAttachmentsToChannel.Configuration;
using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace BPSExt.Teams.CustomActions.AddAtachmentsToChannel.AddAttachmentsToPrivateChannel
{
    public class AddAttachmentsToPrivateChannelConfig : AddAttachmentsToChannelBaseConfig
    {
        [ConfigEditableText(DisplayName = "SharePoint addres", IsRequired = true, Description = "Attachments in private channels are store by additional SharePoint sites." +
            "You can get your SharePoint address by going to Files tab on a chosen team and clicking Copy link. The SharePoint address will look something like: " +
            "https://YourOrganizationName.sharepoint.com/...")]
        public string SpAddres { get; set; }
    }
}