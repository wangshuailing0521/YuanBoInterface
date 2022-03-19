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
    [Description("收款状态更新插件")]
    public class ReceiveBillAudit: AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FSALEDEPTID");
            e.FieldKeys.Add("FBILLTYPEID");
            e.FieldKeys.Add("FDepartment");
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
                sb.AppendLine($@"接口名称：收款状态更新API");
                sb.AppendLine($@"接口类型：状态更新");
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
            string fid = billObj["Id"].ToString();
            string deptId = "";
            DynamicObject dept = billObj["SALEDEPTID"] as DynamicObject;
            if (dept == null)
            {
                dept = billObj["Department"] as DynamicObject;
            }
            if (dept == null)
            {
                return;
            }
            deptId = dept["Id"].ToString();
            string date = billObj["DATE"].ToString();

            InterfaceUrl interfaceUrl
                = SqlHelper.GetCRMUrl(this.Context, deptId);
            if (string.IsNullOrWhiteSpace(interfaceUrl.ReceBillStatusUrl))
            {
                sb.AppendLine($@"{billNo}部门未维护CRM地址");
                Logger.Info("", sb.ToString());
                return;
            }

            string documentStatus
                = SqlHelper.GetReceBillStatus(this.Context, fid);

            int status = 0;
            if (documentStatus == "C")
            {
                status = 3;
            }
            if (documentStatus == "D")
            {
                status = 4;
            }

            if (status == 0)
            {
                return;
            }

            string url = $@"{interfaceUrl.ReceBillStatusUrl}/{billNo}";
            sb.AppendLine($@"Url：{url}");

            var requestInfo = new
            {
                state = status
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
