using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;

using Newtonsoft.Json.Linq;

using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

using YuanBo.K3.Interface.Helper;
using YuanBo.K3.Interface.Model;

namespace YuanBo.K3.Interface
{
    [Description("应收单删除插件")]
    public class ReceivableDelete: AbstractOperationServicePlugIn
    {
        string url = "http://erp.114study.com/api/invoice/delete_bill/";

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            foreach (DynamicObject billObj in e.DataEntitys)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine($@"接口方向：Kingdee --> CRM");
                sb.AppendLine($@"接口名称：应收单删除API");
                sb.AppendLine($@"接口类型：删除");
                try
                {
                    Delete(sb, billObj);
                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message.ToString()}");
                    Logger.Error("", sb.ToString(), ex);

                    throw new Exception(ex.Message.ToString());
                }
            }
        }

        void Delete(StringBuilder sb, DynamicObject billObj)
        {
            string billNo = billObj["BillNo"].ToString();

            url = $@"{url}/{billNo}";
            string json = JsonHelper.ToJSON("");
            sb.AppendLine($@"请求信息：{json}");
            string response = ApiHelper.HttpRequest(url, json, "PUT");
            sb.AppendLine($@"返回信息：{response}");

            #region 解析返回信息
            JObject model = JObject.Parse(response);
            if (model["code"] != null)
            {
                if (model["code"].ToString() != "0")
                {
                    throw new KDException("错误", model["message"].ToString());
                }
            }
            else
            {
                throw new KDException("错误", response);
            }
            #endregion

            Logger.Info("", sb.ToString());
        }
    }
}
