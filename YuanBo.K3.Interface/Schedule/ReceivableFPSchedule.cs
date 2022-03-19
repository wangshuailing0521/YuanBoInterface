using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Text;
using System.Transactions;
using YuanBo.K3.Interface.Helper;
using YuanBo.K3.Interface.Model;

namespace YuanBo.K3.Interface.Schedule
{
    [Description("应收单返回发票号码执行计划")]
    public class ReceivableFPSchedule: IScheduleService
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

                if (string.IsNullOrWhiteSpace(interfaceUrl.FReceivableFPUrl))
                {
                    //sb.AppendLine($@"{billNo}部门未维护CRM地址");
                    //Logger.Info("", sb.ToString());
                    continue;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine($@"接口方向：Kingdee --> CRM");
                sb.AppendLine($@"接口名称：应收单返回发票号码API");
                try
                {
                    //using (KDTransactionScope tran = new KDTransactionScope(TransactionScopeOption.Required))
                    //{
                    Require(item, sb);
                    UpdateStatus(item["FBILLNO"].ToString());
                        
                    //    tran.Complete();
                    //}
                   
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
            string url = $@"http://api.kingdee.114study.com/api/king_dee_invoice_application";

            int status = 0 ;
            string billNo = item["FBILLNO"].ToString();
            string fpNumber = item["F_PAEZ_Text"].ToString();
            string kdNumber = item["F_PAEZ_Text2"].ToString();
            string deptId = item["FSALEDEPTID"].ToString();

            InterfaceUrl interfaceUrl
               = SqlHelper.GetCRMUrl(_ctx, deptId);

            if (string.IsNullOrWhiteSpace(interfaceUrl.FReceivableFPUrl))
            {
                sb.AppendLine($@"{billNo}部门未维护CRM地址");
                Logger.Info("", sb.ToString());
                return;
            }

            url = $@"{interfaceUrl.FReceivableFPUrl}/{billNo}";

            switch (item["FDocumentStatus"].ToString())
            {
                case "C": //已审核
                    status = 3;
                    break;
                case "B": //审核中
                    status = 2;
                    break;
                case "D": //重新审核
                    status = 4;
                    break;
            }

            var requestInfo = new
            {
                state = status,
                invoice_number = fpNumber,
                courier_number = kdNumber
            };

            sb.AppendLine($@"请求Url：{url}");
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

        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        DynamicObjectCollection GetData()
        {
            string sql = $@"
                SELECT  A.FBILLNO
                       ,A.F_PAEZ_Text
                       ,A.F_PAEZ_Text2
                       ,A.FDocumentStatus
                       ,A.FSALEDEPTID
                  FROM  T_AR_RECEIVABLE A
                 WHERE  1=1
                   AND  ISNULL(A.F_PAEZ_Text,'') <> ''
                   AND  ISNULL(A.FFPStatus,'0') = '0'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(_ctx, sql);

            return data;
        }

        /// <summary>
        /// 更新数据接口调用状态
        /// </summary>
        /// <param name="billNo"></param>
        void UpdateStatus(string billNo)
        {
            string sql = $@"
                UPDATE  T_AR_RECEIVABLE
                   SET  FFPStatus = '1'
                 WHERE  FBILLNO='{billNo}'";
            DBUtils.Execute(_ctx, sql);
        }
    }
}
