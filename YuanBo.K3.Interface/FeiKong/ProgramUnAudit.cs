using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using YuanBo.K3.Interface.Helper;
using YuanBo.K3.Interface.Model;

namespace YuanBo.K3.Interface.FeiKong
{
    [Description("项目反审核插件")]
    public class ProgramUnAudit: AbstractOperationServicePlugIn
    {
        string url = "https://api.huilianyi.com/gateway/api/open/costCenterItem";
        string fkId = "";

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

            e.FieldKeys.Add("FId");
            e.FieldKeys.Add("FOid");
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
                DynamicObject type = billObj["FId"] as DynamicObject;
                if (type == null)
                {
                    continue;
                }

                if (type["Number"].ToString() != "01")
                {
                    continue;
                }

                fkId = billObj["FOid"].ToString();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine($@"接口方向：Kingdee --> CRM");
                sb.AppendLine($@"接口名称：项目反审核API");
                try
                {
                    fkId = billObj["FOid"].ToString();
                    if (!string.IsNullOrWhiteSpace(fkId))
                    {
                        Delete(sb, billObj);
                    }
                    else
                    {
                        continue;
                    }

                    Logger.Info("", sb.ToString());
                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message.ToString()}");
                    Logger.Error("", sb.ToString(), ex);

                    throw new Exception(ex.Message.ToString());
                }
            }
        }

        public void Delete(StringBuilder sb, DynamicObject billObj)
        {
            sb.AppendLine($@"接口类型：项目反审核");
            string response = "";
            string json = "";
            FeiKongResponse feiKongResponse = null;

            string dataValue = billObj["DataValue"].ToString();
            string[] dataValues = dataValue.Split('-');
            string dept = dataValues[0];
            string deptNo = SqlHelper.GetHlyDeptNo(this.Context, dept);
            if (deptNo == "")
            {
                throw new KDException("错误", $@"部门名称【{dept}】不存在！");
            }

            string token = GetToken();

            #region 解除成本中心项关联用户/部门/供应商
            sb.AppendLine($@"解除成本中心项关联用户/部门/供应商");
            url = "https://api.huilianyi.com/gateway/api/open/costCenterItem/unrelevance/users";
            url = url + $@"?access_token={token}";

            List<string> depts = new List<string>() { deptNo };

            var requestInfo2 = new
            {
                costCenterCode = "A01",
                code = billObj["Number"].ToString(),
                mode = "1002",
                custDeptNumbers = depts,
                setOfBooksCode = "DEFAULT_SOB",
                employeeIds = new List<string>()
            };

            json = JsonHelper.ToJSON(requestInfo2);
            sb.AppendLine($@"请求地址：{url}");
            sb.AppendLine($@"请求信息：{json}");
            response = ApiHelper.HttpRequest(url, json, "PUT");
            sb.AppendLine($@"返回信息：{response}");

            #region 解析返回信息
            feiKongResponse
                = JsonHelper.FromJSON<FeiKongResponse>(response);

            if (feiKongResponse.errorCode != "0000")
            {
                throw new KDException("错误", feiKongResponse.message);
            }

            #endregion

            #endregion

            #region 删除成本中心项

            sb.AppendLine($@"删除成本中心项");

            url = "https://api.huilianyi.com/gateway/api/open/costCenterItem";
            url = url + $@"?access_token={token}";
            var requestInfo = new
            {
                costCenterCode = "A01",
                code = billObj["Number"].ToString(),
                setOfBooksCode = "DEFAULT_SOB"
            };

            json = JsonHelper.ToJSON(requestInfo);
            sb.AppendLine($@"请求地址：{url}");
            sb.AppendLine($@"请求信息：{json}");
            response = ApiHelper.HttpRequest(url, json, "DELETE");
            sb.AppendLine($@"返回信息：{response}");

            #region 解析返回信息
            feiKongResponse
                = JsonHelper.FromJSON<FeiKongResponse>(response);

            if (feiKongResponse.errorCode != "0000")
            {
                throw new KDException("错误", feiKongResponse.message);
            }
            else
            {
                SqlHelper.UpdateAssistant(this.Context, billObj["Id"].ToString(), "");
                billObj["FOid"] = "";
            }

            #endregion

            #endregion

        }

        public static string GetToken()
        {
            StringBuilder sb = new StringBuilder();
            string url = $@"https://api.huilianyi.com/gateway/oauth/token";

            string appid = "08442fec-491b-43d6-9e11-c91330d8bbf3";
            string appkey = "NDBkOGI5ZmMtYjUxOC00Mjc0LWIyODEtYzU0ZDAwZjk3MDY2";

            sb.AppendLine($@"请求Url：{url}");
            string response = ApiHelper.Post_BasicAuthAsync(url, appid, appkey);
            sb.AppendLine($@"返回信息：{response}");

            JObject jobject = JsonConvert.DeserializeObject<JObject>(response);

            string token = jobject["access_token"].ToString();

            return token;
        }
    }
}
