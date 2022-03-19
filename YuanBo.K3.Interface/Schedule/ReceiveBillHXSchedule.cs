using System;
using System.ComponentModel;
using System.Text;

using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;

using Newtonsoft.Json.Linq;

using YuanBo.K3.Interface.Helper;
using YuanBo.K3.Interface.Model;

namespace YuanBo.K3.Interface.Schedule
{
    [Description("收款单返回核销状态执行计划")]
    public class ReceiveBillHXSchedule: IScheduleService
    {
        Context _ctx;
        public void Run(Context ctx, Kingdee.BOS.Core.Schedule schedule)
        {
            _ctx = ctx;

            ToInterface();
        }

        void ToInterface()
        {
            DynamicObjectCollection data
                = GetData();

            if (data.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < data.Count; i++)
            {
                DynamicObject item = data[i];

                string deptId = item["FSALEDEPTID"].ToString();

                InterfaceUrl interfaceUrl
                   = SqlHelper.GetCRMUrl(_ctx, deptId);

                if (string.IsNullOrWhiteSpace(interfaceUrl.HXUrl))
                {
                    //sb.AppendLine($@"{billNo}部门未维护CRM地址");
                    //Logger.Info("", sb.ToString());
                    continue;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine($@"接口方向：Kingdee --> CRM");
                sb.AppendLine($@"接口名称：收款单返回核销状态API");
                try
                {
                    Require(item, sb);
                    UpdateStatus(item["FBILLNO"].ToString(), item["FWRITTENOFFSTATUS"].ToString());

                    Logger.Info("", sb.ToString());
                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message.ToString()}");
                    Logger.Error("", sb.ToString(), ex);
                }
            }
        }

        /// <summary>
        /// 调用接口
        /// </summary>
        /// <param name="item"></param>
        /// <param name="sb"></param>
        void Require(DynamicObject item, StringBuilder sb)
        {
            string url = $@"http://api.kingdee.114study.com/api/king_dee_common/verification_state";

            int status = 0;
            string billNo = item["FBILLNO"].ToString();
            string hxStatus = item["FWRITTENOFFSTATUS"].ToString();
            string deptId = item["FSALEDEPTID"].ToString();

            InterfaceUrl interfaceUrl
               = SqlHelper.GetCRMUrl(_ctx, deptId);

            if (string.IsNullOrWhiteSpace(interfaceUrl.HXUrl))
            {
                sb.AppendLine($@"{billNo}部门未维护CRM地址");
                Logger.Info("", sb.ToString());
                return;
            }

            url = $@"{interfaceUrl.HXUrl}";

            switch (item["FWRITTENOFFSTATUS"].ToString())
            {
                case "A": //空
                    status = 1;
                    break;
                case "B": //部分
                    status = 2;
                    break;
                case "C": //完全
                    status = 3;
                    break;
            }

            var requestInfo = new
            {
                type = 2,//单据类型(应收单传1，收款单传2)
                number = billNo,
                verificate_state = status
            };

            sb.AppendLine($@"请求Url：{url}");
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

        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        DynamicObjectCollection GetData()
        {
            string sql = $@"
                SELECT  A.FBILLNO
                       ,A.FWRITTENOFFSTATUS
                       ,A.FNEWWRITTENOFFSTATUS
                       ,A.FDocumentStatus
                       ,A.FSALEDEPTID
                  FROM  T_AR_RECEIVEBILL A
                 WHERE  1=1
                   AND  ISNULL(A.FWRITTENOFFSTATUS,'') <> ISNULL(A.FNEWWRITTENOFFSTATUS,'')
                   AND  A.FDocumentStatus = 'C'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(_ctx, sql);

            return data;
        }

        /// <summary>
        /// 更新数据接口调用状态
        /// </summary>
        /// <param name="billNo"></param>
        void UpdateStatus(string billNo, string hxStatus)
        {
            string sql = $@"
                UPDATE  T_AR_RECEIVEBILL
                   SET  FNEWWRITTENOFFSTATUS = '{hxStatus}'
                 WHERE  FBILLNO='{billNo}'";
            DBUtils.Execute(_ctx, sql);
        }
    }
}
