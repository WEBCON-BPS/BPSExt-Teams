using Microsoft.Graph;
using Microsoft.Graph.Models;
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

        internal async Task<List<Exception>> AddAttachmentsToPublicChannelAsync(List<AttachmentData> attachments, AddAttachmentsToChannelBaseConfig config)
        {
            var graphClient = CreateGraphClient();
            var driveId = await GetDriveIdFromPublicChannelAsync(config, graphClient);
            return (await AddAttachmentsToChannelAsync(driveId, config, attachments, graphClient)).ToList();
        }

        internal async Task<List<Exception>> AddAttachmentsToPrivateChannelAsync(List<AttachmentData> attachments, AddAttachmentsToPrivateChannelConfig config)
        {
            var graphClient = CreateGraphClient();
            var driveId = await GetDriveIdFromPrivateChannelAsync(config, graphClient);
            return (await AddAttachmentsToChannelAsync(driveId, config, attachments, graphClient)).ToList();
        }

        private async Task<BlockingCollection<Exception>> AddAttachmentsToChannelAsync(string driveId, AddAttachmentsToChannelBaseConfig config, List<AttachmentData> attachments, GraphServiceClient graphClient)
        {
            var itemId = await GetItemIdAsync(config, graphClient, driveId);
            _logger.AppendLine($"Uploading attachments. DriveId: {driveId}, ItemId: {itemId}");
            await Task.WhenAll(attachments.Select(att => UploadAsync(driveId, itemId, graphClient, att)));
            return uploadExceptions;
        }

        private async Task UploadAsync(string driveId, string itemId, GraphServiceClient graphClient, AttachmentData att)
        {

            _logger.AppendLine($"Uploading {att?.FileName}");
            var content = await att.GetContentAsync();
            using (var stream = new MemoryStream(content))
                await graphClient.Drives[driveId].Items[itemId].ItemWithPath(att.FileName).Content.PutAsync(stream);
                        
        }

        private async Task<string> GetItemIdAsync(AddAttachmentsToChannelBaseConfig config, GraphServiceClient graphClient, string driveId)
        {
            _logger.AppendLine("Downloading ItemId");
            var item = await graphClient.Drives[driveId].Items["root"].Children.GetAsync();
            var channel = item.Value.Where(x => x.Name == config.ChannelName).FirstOrDefault();
            if(channel != null)
                return channel.Id;

            throw new Exception($"Channel with name {config.ChannelName} does not exist. Names found: {string.Join(",", item.Value.Select(x => x.Name))}");
        }

        private async Task<string> GetDriveIdFromPublicChannelAsync(AddAttachmentsToChannelBaseConfig config, GraphServiceClient graphClient)
        {
            _logger.AppendLine("Downloading DriveId");
            var result = await graphClient.Groups[config.TeamId].Drives.GetAsync();
            var drive = result.Value.Where(x => x.DriveType == "documentLibrary").FirstOrDefault();
            if(drive != null)
                return drive.Id;

            throw new Exception($"Cannot find a drive for a group with id: {config.TeamId}");
        }

        private async Task<string> GetDriveIdFromPrivateChannelAsync(AddAttachmentsToPrivateChannelConfig config, GraphServiceClient graphClient)
        {
            var groupName = !string.IsNullOrEmpty(config.TeamName) ? config.TeamName :
                (await graphClient.Groups[config.TeamId].GetAsync()).DisplayName;

            var site = await graphClient.Sites[$"{config.SpAddres}:/sites/{groupName.Replace(" ", "")}-{config.ChannelName.Replace(" ", "")}"].GetAsync();
            var drives = await graphClient.Sites[site.Id].Drives.GetAsync();
            return drives.Value.Where(x => x.DriveType == "documentLibrary").First().Id;
        }

    }
}
