using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCon.BpsExt.Teams.CustomActions.GraphApi;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;

namespace WebCon.BpsExt.Teams.CustomActions.AddAtachmentsToChannel.AddAttachmentsToPrivateChannel
{
    public class AddAttachmentsToPrivateChannel : CustomAction<AddAttachmentsToPrivateChannelConfig>
    {
        StringBuilder _logger = new StringBuilder();
        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                var attachments = await new AttachmentsHelper().GetAttachmentsAsync(args.Context, Configuration);
                var graphProvider = new GraphApiAttachmentsHelper(Configuration, _logger, args.Context);
                var exceptions = await graphProvider.AddAttachmentsToPrivateChannelAsync(attachments, Configuration);
                CheckExceptions(exceptions, args);
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

        private void CheckExceptions(List<Exception> exceptions, RunCustomActionParams args)
        {
            if (exceptions.Any())
            {
                _logger.AppendLine("Exception while uploading files");
                foreach (var ex in exceptions)
                {
                    _logger.AppendLine(ex.ToString());
                }
                _logger.AppendLine(exceptions.FirstOrDefault().ToString());
                args.Message = exceptions.FirstOrDefault().Message;
                args.HasErrors = true;
            }
        }
    }
}