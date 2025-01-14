using System;
using System.Text;
using System.Threading.Tasks;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace WebCon.BpsExt.Teams.CustomActions.AddAtachmentsToChannel
{
    public class AddAttachmentsToChannel : CustomAction<AddAttachmentToChannelConfig>
    {
        StringBuilder _logger = new StringBuilder();
        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                var attachments = await new AttachmentsHelper().GetAttachmentsAsync(args.Context, Configuration);
                var graphProvider = new GraphApiAttachmentsHelper(Configuration, _logger, args.Context);
                await graphProvider.AddAttachmentsToChannelAsync(attachments, Configuration);
            }
            catch (Exception ex)
            {
                _logger.AppendLine(ex.ToString());
                args.Message = ex.Message;
                args.HasErrors = true;
            }
            finally
            {
                args.Context.PluginLogger.AppendInfo(_logger.ToString());
                args.LogMessage = _logger.ToString();
            }
        }
    }
}