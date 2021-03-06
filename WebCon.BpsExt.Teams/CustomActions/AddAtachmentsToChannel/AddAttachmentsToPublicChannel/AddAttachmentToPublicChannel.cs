using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebCon.BpsExt.Teams.CustomActions.AddAttachmentsToChannel.Configuration;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace WebCon.BpsExt.Teams.CustomActions.AddAtachmentsToChannel.AddAttachmentsToPublicChannel
{
    public class AddAttachmentsToPublicChannel : CustomAction<AddAttachmentsToChannelBaseConfig>
    {
        StringBuilder _logger = new StringBuilder();
        public override void Run(RunCustomActionParams args)
        {
            try
            {
                var attachments = new AttachmentsHelper().GetAttachments(args.Context, Configuration);
                var graphProvider = new GraphApiAttachmentsHelper(Configuration, _logger, args.Context);
                var exceptions = graphProvider.AddAttachmentsToPublicChannel(attachments, Configuration);
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
            if(exceptions.Any())
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