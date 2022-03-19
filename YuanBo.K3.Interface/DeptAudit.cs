using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using YuanBo.K3.Interface.Helper;

namespace YuanBo.K3.Interface
{
    [Description("部门审核插件")]
    public class DeptAudit:AbstractOperationServicePlugIn
    {
        string url = "http://api.kingdee.114study.com/api/king_dee_departments";
        string crmId = "";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FCreateOrgId");
            e.FieldKeys.Add("FUseOrgId");
            e.FieldKeys.Add("FCRMId");
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
                sb.AppendLine($@"接口名称：部门审核API");
                try
                {
                    crmId = billObj["FCRMId"].ToString();
                    if (string.IsNullOrWhiteSpace(crmId))
                    {
                        Add(sb, billObj);
                    }
                    else
                    {
                        Edit(sb, billObj);
                    }
                  
                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message.ToString()}");
                    Logger.Error("", sb.ToString(), ex);

                    throw new Exception(ex.Message.ToString());
                }
            }
        }

        public void Add(StringBuilder sb, DynamicObject billObj)
        {
            sb.AppendLine($@"接口类型：新增");

            DynamicObject createOrg = billObj["CreateOrgId"] as DynamicObject;
            DynamicObject useOrg = billObj["UseOrgId"] as DynamicObject;

            var requestInfo = new
            {
                king_dee_department_no = billObj["Number"].ToString(),
                king_dee_department_name = billObj["Name"].ToString(),
                create_no = createOrg["Number"].ToString(),
                use_no = useOrg["Number"].ToString()
            };

            string json = JsonHelper.ToJSON(requestInfo);
            sb.AppendLine($@"请求信息：{json}");
            string response = ApiHelper.HttpRequest(url, json,"POST");
            sb.AppendLine($@"返回信息：{response}");

            #region 解析返回信息
            JObject model = JObject.Parse(response);
            if (model["code"] != null)
            {
                if (model["code"].ToString() != "0")
                {
                    throw new KDException("错误", model["message"].ToString());
                }
                else
                {
                    JObject data
                        = JObject.Parse(model["data"].ToString());
                    string crmId = data["id"].ToString();
                    SqlHelper.UpdateDept(this.Context, billObj["Id"].ToString(), crmId);
                    billObj["FCRMId"] = crmId;
                }
            }
            else
            {
                throw new KDException("错误", response);
            }
            #endregion

            Logger.Info("", sb.ToString());
        }

        public void Edit(StringBuilder sb, DynamicObject billObj)
        {
            sb.AppendLine($@"接口类型：修改");
            

            string number = billObj["Number"].ToString();
            string name = billObj["Name"].ToString();
            DynamicObject createOrg = billObj["CreateOrgId"] as DynamicObject;
            DynamicObject useOrg = billObj["UseOrgId"] as DynamicObject;

            url = $@"http://api.kingdee.114study.com/api/king_dee_departments/{number}";
            var requestInfo = new
            {
                king_dee_department_name = billObj["Name"].ToString(),
                state= 0,
                create_no = createOrg["Number"].ToString(),
                use_no = useOrg["Number"].ToString()
            };

            string json = JsonHelper.ToJSON(requestInfo);
            sb.AppendLine($@"请求信息：{json}");
            string response = ApiHelper.HttpRequest(url, json,"PUT");
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
