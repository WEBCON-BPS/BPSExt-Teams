using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebCon.BpsExt.Teams.CustomActions.Models;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace WebCon.BpsExt.Teams.CustomActions.CreateTeamsChannel
{
    public class CreateTeamsChannel : CustomAction<CreateTeamsChannelConfig>
    {
        StringBuilder _logger = new StringBuilder();

        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                var helper =  new BpsApiHelper(Configuration, args.Context, _logger);
                var privilages = await helper.GetWorkflofInstancePrivilegesAsync(args.Context.CurrentDocument.ID);
                await CreateChannelAsync(privilages, args.Context);
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

        private async Task CreateChannelAsync(List<ElementPrivileges> users, ActionContextInfo context)
        {
            var graphProvider = new GraphApiTeamHelper(Configuration, context, _logger);
            var dataToSave = await graphProvider.CreateTeamsChannelAsync(users);
            await SaveOnFormAsync(dataToSave, context);
        }

        private async Task SaveOnFormAsync(DataToSave data, ActionContextInfo context)
        {
            _logger.AppendLine("Saving data to fileds");
            await SaveIfDefinedAsync(Configuration.AdditionalConfig.ChannelIdFieldId, data.ChannelId, context);
            await SaveIfDefinedAsync(Configuration.AdditionalConfig.ChannelUrlFieldId, data.ChannelWebUrl, context);
            await SaveIfDefinedAsync(Configuration.AdditionalConfig.TeamIdFieldId, data.TeamId, context);
        }

        private async Task SaveIfDefinedAsync(int? fieldId, string value, ActionContextInfo context)
        {
            if (fieldId != null)
                await context.CurrentDocument.SetFieldValueAsync((int)fieldId, value);

        }
    }
}
