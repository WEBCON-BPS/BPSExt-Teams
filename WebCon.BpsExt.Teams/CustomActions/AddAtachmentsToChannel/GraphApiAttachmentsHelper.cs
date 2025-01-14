using Microsoft.Graph;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCon.BpsExt.Teams.CustomActions.GraphApi;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;

namespace WebCon.BpsExt.Teams.CustomActions.AddAtachmentsToChannel
{
    public class GraphApiAttachmentsHelper : GraphApiProvider
    {
        public GraphApiAttachmentsHelper(AddAttachmentToChannelConfig config, StringBuilder log, ActionContextInfo context) : base(config.ConnectionId, log, context)
        {

        }

        internal async Task AddAttachmentsToChannelAsync(List<AttachmentData> attachments, AddAttachmentToChannelConfig config)
        {
            var graphClient = CreateGraphClient(config.UseProxy);
            await UploadFilesToChannelAsync(config, graphClient, attachments);
        }

        private async Task UploadFilesToChannelAsync(AddAttachmentToChannelConfig config, GraphServiceClient graphClient, List<AttachmentData> attachments)
        {
            var filesFolder = await graphClient
                .Teams[config.TeamId]
                .Channels[$"{config.ChannelId.Replace(" ", "")}"]
                .FilesFolder.GetAsync();

            await Task.WhenAll(attachments.Select(att => UploadAsync(filesFolder.ParentReference.DriveId, filesFolder.Id, graphClient, att)));
        }

        private async Task UploadAsync(string driveId, string itemId, GraphServiceClient graphClient, AttachmentData att)
        {
            _logger.AppendLine($"Uploading {att?.FileName}");
            var content = await att.GetContentAsync();
            using (var stream = new MemoryStream(content))
                await graphClient.Drives[driveId].Items[itemId].ItemWithPath(att.FileName).Content.PutAsync(stream);
        }
    }
}
