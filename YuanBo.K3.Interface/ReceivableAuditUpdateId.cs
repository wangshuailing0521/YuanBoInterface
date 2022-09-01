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
    [Description("应收单审核传递收款计划ID插件")]
    public class ReceivableAuditUpdateId: AbstractOperationServicePlugIn
    {
        string url = "https://erp.114study.com/api/king_dee_invoice_application/update_plan_id";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FSALEDEPTID");
            e.FieldKeys.Add("FPAYAMOUNTFOR");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);

            if (e.DataEntitys.Count() < 1)
            {
                return;
            }

            foreach (DynamicObject billObj in e.DataEntitys)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine($@"接口方向：Kingdee --> CRM");
                sb.AppendLine($@"接口名称：应收单审核传递收款计划IDAPI");
                sb.AppendLine($@"接口类型：审核");
                try
                {
                    Audit(sb, billObj);
                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message}");
                    Logger.Error("", sb.ToString(), ex);

                    throw new Exception(ex.Message.ToString());
                }
            }
        }

        public void Audit(StringBuilder sb, DynamicObject billObj)
        {
            string billNo = billObj["BillNo"].ToString();
            DynamicObjectCollection planEntrys = billObj["AP_PAYABLEPLAN"] as DynamicObjectCollection;
            string planId = planEntrys[0]["Id"].ToString();

            DynamicObject dept = billObj["SALEDEPTID"] as DynamicObject;
            string deptId = dept["Id"].ToString();

            InterfaceUrl interfaceUrl
                = SqlHelper.GetCRMUrl(this.Context, deptId);

            if (string.IsNullOrWhiteSpace(interfaceUrl.ReceivablePlanUrl))
            {
                sb.AppendLine($@"{billNo}部门未维护CRM收款计划更新地址");
                Logger.Info("", sb.ToString());
                return;
            }

            url = $@"{interfaceUrl.ReceivablePlanUrl}";

            sb.AppendLine($@"Url：{url}");

            var requestInfo = new
            {
                king_dee_invoice_plan_number = planId,
                king_dee_invoice_number = billNo,
            };

            string json = JsonHelper.ToJSON(requestInfo);
            sb.AppendLine($@"请求信息：{json}");
            string response = ApiHelper.HttpRequest(url, json, "POST");
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
