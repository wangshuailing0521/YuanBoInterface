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
    [Description("应收单审核插件")]
    public class ReceivableAudit: AbstractOperationServicePlugIn
    {
        string url = "http://api.kingdee.114study.com/api/king_dee_invoice_application/";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FSALEDEPTID");
            e.FieldKeys.Add("F_PAEZ_Text");
            e.FieldKeys.Add("F_PAEZ_Text2");
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
                sb.AppendLine($@"接口名称：应收单状态更新API");
                sb.AppendLine($@"接口类型：审核");
                try
                {
                    Audit(sb, billObj);
                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message.ToString()}");
                    Logger.Error("", sb.ToString(), ex);

                    throw new Exception(ex.Message.ToString());
                }
            }
        }

        public void Audit(StringBuilder sb, DynamicObject billObj)
        {
            string billNo = billObj["BillNo"].ToString();
            string fpNumber = billObj["F_PAEZ_Text"].ToString();
            string kdNumber = billObj["F_PAEZ_Text2"].ToString();
            DynamicObject dept = billObj["SALEDEPTID"] as DynamicObject;
            string deptId = dept["Id"].ToString();

            InterfaceUrl interfaceUrl
                = SqlHelper.GetCRMUrl(this.Context, deptId);

            if (string.IsNullOrWhiteSpace(interfaceUrl.ReceivableUrl))
            {
                sb.AppendLine($@"{billNo}部门未维护CRM地址");
                Logger.Info("", sb.ToString());
                return;
            }

            url = $@"{interfaceUrl.ReceivableUrl}/{billNo}";

            sb.AppendLine($@"Url：{url}");

            var requestInfo = new
            {
                state = 3,
                invoice_number = fpNumber,
                courier_number = kdNumber
            };

            string json = JsonHelper.ToJSON(requestInfo);
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
