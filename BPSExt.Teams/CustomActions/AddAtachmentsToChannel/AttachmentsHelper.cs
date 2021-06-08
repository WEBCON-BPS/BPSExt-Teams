using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using BPSExt.Teams.CustomActions.AddAttachmentsToChannel.Configuration;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;

namespace BPSExt.Teams.CustomActions.AddAtachmentsToChannel
{
    public class AttachmentsHelper
    {
        public List<AttachmentData> GetAttachments(ActionContextInfo context, AddAttachmentsToChannelBaseConfig Configuration)
        {
            var attachments = new List<AttachmentData>();
            var ids = WebCon.WorkFlow.SDK.Tools.Data.SqlExecutionHelper.GetDataTableForSqlCommand(Configuration.AttachmentsQuery, context);
            foreach (DataRow row in ids.Rows)
            {
                var id = row.Field<int>(0);
                var att = context.CurrentDocument.Attachments.GetByID(id);
                attachments.Add(att);
            }
            return attachments;
        }
    }
}
