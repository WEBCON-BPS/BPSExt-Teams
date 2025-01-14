using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using WebCon.WorkFlow.SDK.Tools.Data;

namespace WebCon.BpsExt.Teams.CustomActions.AddAtachmentsToChannel
{
    public class AttachmentsHelper
    {
        public async Task<List<AttachmentData>> GetAttachmentsAsync(ActionContextInfo context, AddAttachmentToChannelConfig Configuration)
        {
            var attachments = new List<AttachmentData>();
            var ids = await new SqlExecutionHelper(context).GetDataTableForSqlCommandAsync(Configuration.AttachmentsQuery);
            foreach (DataRow row in ids.Rows)
            {
                var id = row.Field<int>(0);
                var att = await context.CurrentDocument.Attachments.GetByIDAsync(id);
                attachments.Add(att);
            }
            return attachments;
        }
    }
}
