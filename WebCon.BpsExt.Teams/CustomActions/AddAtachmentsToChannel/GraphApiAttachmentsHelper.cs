using Microsoft.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCon.BpsExt.Teams.CustomActions.AddAtachmentsToChannel.AddAttachmentsToPrivateChannel;
using WebCon.BpsExt.Teams.CustomActions.AddAttachmentsToChannel.Configuration;
using WebCon.BpsExt.Teams.CustomActions.GraphApi;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;

namespace WebCon.BpsExt.Teams.CustomActions.AddAtachmentsToChannel
{
    public class GraphApiAttachmentsHelper : GraphApiProvider
    {
        BlockingCollection<Exception> uploadExceptions = new BlockingCollection<Exception>();

        public GraphApiAttachmentsHelper(AddAttachmentsToChannelBaseConfig config, StringBuilder log, ActionContextInfo context) : base(config.ConnectionId, log, context)
        {

        }

        internal List<Exception> AddAttachmentsToPublicChannel(List<AttachmentData> attachments, AddAttachmentsToChannelBaseConfig config)
        {
            var graphClient = CreateGraphClient();
            var driveId = GetDriveIdFromPublicChannel(config, graphClient);
            return AddAttachmentsToChannel(driveId, config, attachments, graphClient).Result.ToList();
        }

        internal List<Exception> AddAttachmentsToPrivateChannel(List<AttachmentData> attachments, AddAttachmentsToPrivateChannelConfig config)
        {
            var graphClient = CreateGraphClient();
            var driveId = GetDriveIdFromPrivateChannel(config, graphClient);
            return AddAttachmentsToChannel(driveId, config, attachments, graphClient).Result.ToList();
        }

        private async Task<BlockingCollection<Exception>> AddAttachmentsToChannel(string driveId, AddAttachmentsToChannelBaseConfig config, List<AttachmentData> attachments, GraphServiceClient graphClient)
        {
            var itemId = GetItemId(config, graphClient, driveId);
            await Task.WhenAll(attachments.Select(att => UploadAsync(driveId, itemId, graphClient, att)));
            return uploadExceptions;
        }

        private async Task UploadAsync(string driveId, string itemId, GraphServiceClient graphClient, AttachmentData att)
        {
            var sessionResponse = await graphClient.Drives[driveId].Items[itemId].ItemWithPath(att.FileName).CreateUploadSession().Request().PostAsync();
            await UploadBySession(sessionResponse, graphClient, att);
        }

        private async Task UploadBySession(UploadSession sessionResponse, GraphServiceClient graphClient, AttachmentData att)
        {
            using (var stream = new MemoryStream(att.Content))
            {
                var provider = new ChunkedUploadProvider(sessionResponse, graphClient, stream);
                var chunkRequests = provider.GetUploadChunkRequests();
                var trackedExceptions = new List<Exception>();
                DriveItem itemResult = null;

                foreach (var request in chunkRequests)
                {
                    var result = await provider.GetChunkRequestResponseAsync(request, trackedExceptions);
                    if (result.UploadSucceeded)
                        itemResult = result.ItemResponse;
                }
                foreach (Exception ex in trackedExceptions)
                    uploadExceptions.Add(ex);                 
            }
        }

        private string GetItemId(AddAttachmentsToChannelBaseConfig config, GraphServiceClient graphClient, string driveId)
        {
            _logger.AppendLine("Downloading ItemId");
            var item = graphClient.Drives[driveId].Root.Children.Request().GetAsync().Result;
            return item.Where(x => x.Name == config.ChannelName).FirstOrDefault().Id;
        }

        private string GetDriveIdFromPublicChannel(AddAttachmentsToChannelBaseConfig config, GraphServiceClient graphClient)
        {
            _logger.AppendLine("Downloading DriveId");
            return graphClient.Groups[config.TeamId].Drives.Request().GetAsync()
                .Result.Where(x => x.DriveType == "documentLibrary").First().Id;
        }

        private string GetDriveIdFromPrivateChannel(AddAttachmentsToPrivateChannelConfig config, GraphServiceClient graphClient)
        {
            var groupName = !string.IsNullOrEmpty(config.TeamName) ? config.TeamName :
                graphClient.Groups[config.TeamId].Request().GetAsync().Result.DisplayName;

            var site = graphClient.Sites.GetByPath($"/sites/{groupName.Replace(" ", "")}-{config.ChannelName.Replace(" ", "")}", config.SpAddres).Request().GetAsync().Result;
            var drives = graphClient.Sites[site.Id].Drives.Request().GetAsync().Result;
            return drives.Where(x => x.DriveType == "documentLibrary").First().Id;
        }

    }
}
