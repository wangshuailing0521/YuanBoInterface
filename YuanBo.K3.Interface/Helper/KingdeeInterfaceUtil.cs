using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanBo.K3.Interface.Model;

namespace YuanBo.K3.Interface.Helper
{
    public class KingdeeInterfaceUtil
    {
        private static string loginUrl = "http://120.26.93.232/k3cloud/Kingdee.BOS.WebApi.ServicesStub.AuthService.ValidateUser.common.kdsvc";
        private static string saveUrl = "http://120.26.93.232/K3Cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save.common.kdsvc";
        private static string submitUrl = "http://120.26.93.232/K3Cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Submit.common.kdsvc";
        private static string auditUrl = "http://120.26.93.232/K3Cloud/Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Audit.common.kdsvc";

        public static void Sync1(string formId, string content)
        {
            try
            {
                ApiClient client = new ApiClient("http://120.26.93.232/k3cloud/");
                string dbId = "5fa2ba407f5078"; //1104
                bool bLogin = client.Login(dbId, "admin", "YBJT1920", 2052);
                if (bLogin)
                {
                    //todo:登陆成功处理业务 
                    //业务对象Id
                    //string sFormId = "SAL_OUTSTOCK";  //销售出库单  
                    //                                  //Model字串 
                    //string sContent = "{\"Creator\":\"\",\"NeedUpDateFields\":[],\"Model\":" + "{\"FID\":\"0\",\"FStockOrgId\":{\"FNumber\":\"210\"},\"FBillTypeID\":{\"FNumber\":\"XSCKD01_SYS\"},\"FBillNo\":\"CSDGBC21002\",\"FCustomerID\":{\"FNumber\":\"CUST0073\"},\"SubHeadEntity\":{\"FExchangeRate\":6.51},\"FEntity\":[{\"FEntryID\":\"0\",\"FMATERIALID\":{\"FNumber\":\"03.001\"},\"FStockID\":{\"FNumber\":\"CK002\"},\"FRealQty\":324,\"FBaseUnitQty\":324},{\"FEntryID\":\"0\",\"FMATERIALID\":{\"FNumber\":\"03.001\"},\"FStockID\":{\"FNumber\":\"CK004\"},\"FRealQty\":220,\"FBaseUnitQty\":220}]]}}";
                    object[] saveInfo = new object[]
                    {
                        formId,
                        content
                    };
                    //调用保存接口 
                    var ret = client.Execute<string>("Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save", saveInfo);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static string Sync(
            string formId, 
            string content, 
            string number, 
            string dbid, //账套内码
            bool autoSubmitAndAudit = true)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                //string dbid = "5c467fddadea41";
                string user = "admin";
                string password = "YBJT1920";

                KingdeeHttpClient httpClient = new KingdeeHttpClient();
                httpClient.Url = loginUrl;
                List<object> Parameters = new List<object>();
                Parameters.Add(dbid);
                Parameters.Add(user);
                Parameters.Add(password);
                Parameters.Add(2052);
                httpClient.Content = JsonConvert.SerializeObject(Parameters);
                var iResult = JObject.Parse(httpClient.AsyncRequest())["LoginResultType"].Value<int>();
                if (iResult == 1)
                {
                    httpClient.Url = saveUrl;
                    Parameters = new List<object>();
                    Parameters.Add(formId);
                    Parameters.Add(content);
                    httpClient.Content = JsonConvert.SerializeObject(Parameters);
                    var responseOut = httpClient.AsyncRequest();
                    KingdeeSaveResponse response = JsonConvert.DeserializeObject<KingdeeSaveResponse>(responseOut);

                    ResponseStatus status = response.Result.ResponseStatus;
                    if (status.IsSuccess && autoSubmitAndAudit)
                    {
                        //提交单据
                        httpClient.Url = submitUrl;
                        Parameters = new List<object>();
                        Parameters.Add(formId);
                        var respuest = new
                        {
                            CreateOrgId = 0,
                            Numbers = new List<string>() { number }
                        };
                        Parameters.Add(JsonConvert.SerializeObject(respuest));
                        httpClient.Content = JsonConvert.SerializeObject(Parameters);
                        responseOut = httpClient.AsyncRequest();
                        KingdeeSubmitResponse submitResponse 
                            = JsonConvert.DeserializeObject<KingdeeSubmitResponse>(responseOut);
                        status = submitResponse.Result.ResponseStatus;

                        if (status.IsSuccess)
                        {
                            
                        }
                        else
                        {
                            foreach (var item in status.Errors)
                            {
                                sb.AppendLine(item.Message);
                            }
                        }

                        //审核单据
                        httpClient.Url = auditUrl;
                        httpClient.Content = JsonConvert.SerializeObject(Parameters);
                        responseOut = httpClient.AsyncRequest();
                        KingdeeSubmitResponse auditResponse
                            = JsonConvert.DeserializeObject<KingdeeSubmitResponse>(responseOut);
                        status = submitResponse.Result.ResponseStatus;

                        if (!status.IsSuccess)
                        {
                            foreach (var item in status.Errors)
                            {
                                sb.AppendLine(item.Message);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in status.Errors)
                        {
                            sb.AppendLine(item.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.Message);
            }

            return sb.ToString();
        }
    }
}
