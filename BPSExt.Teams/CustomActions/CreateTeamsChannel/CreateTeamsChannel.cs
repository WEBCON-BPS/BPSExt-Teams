using System;
using System.Collections.Generic;
using System.Text;
using BPSExt.Teams.CustomActions.Models;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace BPSExt.Teams.CustomActions.CreateTeamsChannel
{
    public class CreateTeamsChannel : CustomAction<CreateTeamsChannelConfig>
    {
        StringBuilder _logger = new StringBuilder();

        public override void Run(RunCustomActionParams args)
        {
            try
            {
                var privilages = new BpsApiHelper(Configuration, args.Context).GetWorkflofInstancePrivileges(args.Context.CurrentDocument.ID);
                CreateChannel(privilages, args.Context);
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

        private void CreateChannel(List<ElementPrivileges> users, ActionContextInfo context)
        {
            var graphProvider = new GraphApiTeamHelper(Configuration, context, _logger);
            var dataToSave = graphProvider.CreateTeamsChannel(users);
            SaveOnForm(dataToSave, context);
        }

        private void SaveOnForm(DataToSave data, ActionContextInfo context)
        {
            _logger.AppendLine("Saving data to fileds");
            SaveIfDefined(Configuration.AdditionalConfig.ChannelIdFieldId, data.ChannelId, context);
            SaveIfDefined(Configuration.AdditionalConfig.ChannelUrlFieldId, data.ChannelWebUrl, context);
            SaveIfDefined(Configuration.AdditionalConfig.TeamIdFieldId, data.TeamId, context);
        }

        private void SaveIfDefined(int? fieldId, string value, ActionContextInfo context)
        {
            if (fieldId != null)
                context.CurrentDocument.SetFieldValue((int)fieldId, value);

        }
    }
}
