using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using YuanBo.K3.Interface.Helper;
using YuanBo.K3.Interface.Model;

namespace YuanBo.K3.Interface.Schedule.FK
{
    [Description("获取汇联易数据执行计划")]
    public class GetSupplierSchedule : IScheduleService
    {
        Context _ctx;
        string _dbid;

        public void Run(Context ctx, Kingdee.BOS.Core.Schedule schedule)
        {
            string url = ctx.ServerUrl;
            _dbid = ctx.DBId;

            Logger.Info("", $@"ServerUrl:{url}");

            _ctx = ctx;

            //GetDept();

            GetSupplier();

            GetUser();

            Logger.Info("", "获取汇联易数据成功");
        }

        public void GetSupplier()
        {
            int pages = 1;
            int counts = 0;
            for (int i = 1; i <= pages; i++)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine($@"接口账套：{_dbid}");
                sb.AppendLine($@"接口方向：HLY --> KINGDEE");
                sb.AppendLine($@"接口名称：获取供应商API");
                try
                {
                    string token = GetToken();
                    string url = $@"https://api.huilianyi.com/gateway/api/open/vendor";
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic.Add("access_token", token);
                    dic.Add("startDate", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                    dic.Add("endDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    dic.Add("page", i);
                    dic.Add("size", 50);
                    dic.Add("allVendor", true);

                    string request = JsonHelper.ToJSON(dic);
                    sb.AppendLine($@"请求信息：{request}");
                    string result = ApiHelper.HttpGet(url, dic, out counts);
                    pages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(counts / 50)));

                    sb.AppendLine($@"返回信息：{result}");
                    sb.AppendLine($@"行数：{counts}");


                    List<HLYSupplier> list
                        = JsonConvert.DeserializeObject<List<HLYSupplier>>(result);

                    for (int j = 0; j < list.Count; j++)
                    {
                        HLYSupplier hly = list[j];

                        if (hly.vendorTypeCode == "01")
                        {
                            hly.vendorTypeCode = "1002";
                        }

                        if (hly.vendorTypeCode == "02")
                        {
                            hly.vendorTypeCode = "1001";
                        }

                        string vendorNo = hly.vendorNumber;
                        if (SqlHelper.HaveSupplier(_ctx, vendorNo))
                        {
                            int count = 0;

                            //更新供应商分组，分类
                            count = SqlHelper.UpdateSupplierGroup(_ctx, hly.vendorTypeCode, vendorNo);
                            if (count > 0)
                                Logger.Info("", $@"更新供应商分组【{vendorNo}】【{hly.vendorTypeCode}】");
                            count = SqlHelper.UpdateSupplierType(_ctx, hly.vendorTypeCode, vendorNo);
                            if (count > 0)
                                Logger.Info("", $@"更新供应商分类【{vendorNo}】【{hly.vendorTypeCode}】");

                            if (hly.status == "1002")
                            {
                                count = SqlHelper.ForbidSupplier(_ctx, vendorNo);
                                if (count > 0)
                                {
                                    Logger.Info("", $@"禁用供应商【{hly.vendorNumber}】");
                                }
                                
                            }

                            if (hly.status == "1001")
                            {
                                count = SqlHelper.UnForbidSupplier(_ctx, vendorNo);
                                if (count > 0)
                                {
                                    Logger.Info("", $@"反禁用供应商【{hly.vendorNumber}】");
                                }

                                #region 银行信息管理
                                foreach (var bankAccount in hly.bankAccounts)
                                {
                                    Logger.Info("", $@"供应商银行账号【{bankAccount.bankAccount}】");
                                    string supplerId = SqlHelper.GetSupplierBankInfo(_ctx, vendorNo, bankAccount.bankAccount);
                                    if (string.IsNullOrWhiteSpace(supplerId))
                                    {
                                        Logger.Info("", $@"新增供应商银行账号【{bankAccount.bankAccount}】");
                                        // 获取主键值
                                        var ids = Kingdee.BOS.ServiceHelper.DBServiceHelper.GetSequenceInt64(_ctx,
                                                "t_BD_SupplierBank",
                                                1);

                                        string entryId = ids.ElementAt(0).ToString();

                                        SqlHelper.AddSupplierBankInfo(_ctx, 
                                            supplerId, 
                                            entryId, 
                                            bankAccount.bankAccount, 
                                            bankAccount.venBankNumberName, 
                                            bankAccount.bankOpeningBank);

                                        List<string> supplierIds = SqlHelper.GetSupplierIds(_ctx, vendorNo);
                                        foreach (var item in supplierIds)
                                        {
                                            var ids1 = Kingdee.BOS.ServiceHelper.DBServiceHelper.GetSequenceInt64(_ctx,
                                                "t_BD_SupplierBank",
                                                1);

                                            string entryId1 = ids.ElementAt(0).ToString();

                                            supplerId = item;

                                            SqlHelper.AddSupplierBankInfo(_ctx,
                                                supplerId,
                                                entryId1,
                                                bankAccount.bankAccount,
                                                bankAccount.venBankNumberName,
                                                bankAccount.bankOpeningBank,
                                                entryId);
                                        }
                                    }
                                }
                                #endregion
                            }

                            continue;
                        }
                        else
                        {
                            if (hly.status == "1002")
                            {
                                continue;
                            }
                        }

                        KingdeeSupplierModel model = new KingdeeSupplierModel();
                        //model.FSupplierId = Convert.ToInt32(id);
                        model.FCreateOrgId = new BaseModel { FNumber = "100" };
                        model.FUseOrgId = new BaseModel { FNumber = "100" };
                        model.FNumber = vendorNo;
                        model.FName = hly.vendorName;
                        model.FGroup = new BaseModel { FNumber = hly.vendorTypeCode };

                        FSupplierBase sbase = new FSupplierBase();
                        sbase.FSupplierClassify = new BaseModel { FNumber = hly.vendorTypeCode };
                        model.FBaseInfo = sbase;

                        FSupplierBusinessInfo sbusiness = new FSupplierBusinessInfo();
                        sbusiness.FSettleTypeId = new BaseModel { FNumber = "JSFS05_SYS" };
                        model.FBusinessInfo = sbusiness;

                        FSupplierFFinanceInfo sfin = new FSupplierFFinanceInfo();
                        sfin.FPayCurrencyId = new BaseModel { FNumber = "PRE001" };
                        model.FFinanceInfo = sfin;

                        List<FSupplierBank> banks = new List<FSupplierBank>(); 
                        foreach (var bankAccount in hly.bankAccounts)
                        {
                            FSupplierBank fSupplierBank = new FSupplierBank();

                            fSupplierBank.FBankCode = bankAccount.bankAccount;
                            fSupplierBank.FBankHolder = bankAccount.venBankNumberName;
                            fSupplierBank.FOpenBankName = bankAccount.bankOpeningBank;

                            banks.Add(fSupplierBank);
                        }

                        if (banks.Count>0)
                        {
                            model.FBankInfo = banks;
                        }
                        

                        string formId = "BD_Supplier";
                        InterfaceModel interfaceModel = new InterfaceModel() { Model = model };
                        string content = JsonHelper.ToJSON(interfaceModel);
                        string response
                            = KingdeeInterfaceUtil.Sync(formId, content, vendorNo, _dbid);

                        Logger.Info("", $@"供应商保存返回信息【{response}】");
                        if (!string.IsNullOrWhiteSpace(response))
                        {

                        }
                    }


                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message}");
                }
                finally
                {
                    Logger.Info("", sb.ToString());
                }


            }

        }

        public void GetDept()
        {
            int pages = 1;
            for (int i = 1; i <= pages; i++)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine($@"接口方向：HLY --> KINGDEE");
                sb.AppendLine($@"接口名称：获取部门API");

                try
                {
                    string token = GetToken();
                    string url = $@"https://api.huilianyi.com/gateway/api/open/department";
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic.Add("access_token", token);
                    dic.Add("startDate", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                    dic.Add("endDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    dic.Add("page", i);
                    dic.Add("size", 50);

                    string request = JsonHelper.ToJSON(dic);
                    sb.AppendLine($@"请求信息：{request}");
                    string result = ApiHelper.HttpGet(url, dic, out pages);

                    sb.AppendLine($@"返回信息：{result}");


                    List<HLYDept> list = JsonHelper.FromJSON<List<HLYDept>>(result);

                    for (int j = 0; j < list.Count; j++)
                    {
                        HLYDept hly = list[j];
                        SqlHelper.UpdateHlyDept(_ctx, hly.departmentName, hly.custDeptNumber);
                    }

                    Logger.Info("", sb.ToString());

                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message}");
                    Logger.Info("", sb.ToString());
                }
            }

        }

        public void GetUser()
        {
            int pages = 1;
            int counts = 0;

            string formId = "";
            string content = "";
            string response = "";
                
            for (int i = 1; i <= pages; i++)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("");
                sb.AppendLine($@"接口账套：{_dbid}");
                sb.AppendLine($@"接口方向：HLY --> KINGDEE");
                sb.AppendLine($@"接口名称：获取员工API");
                try
                {
                    string token = GetToken();
                    string url = $@"https://api.huilianyi.com/gateway/api/open/user";
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    dic.Add("access_token", token);
                    dic.Add("startDate", DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss"));
                    dic.Add("endDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    //dic.Add("startDate", "2021-04-01 00:00:00");
                    //dic.Add("endDate", "2021-04-02 00:00:00");
                    dic.Add("showCustomValues", true);
                    dic.Add("page", i);
                    dic.Add("size", 50);

                    string request = JsonHelper.ToJSON(dic);
                    sb.AppendLine($@"请求信息：{request}");
                    string result = ApiHelper.HttpGet(url, dic, out counts);

                    pages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(counts / 50)));
                    sb.AppendLine($@"返回信息：{result}");
                    sb.AppendLine($@"行数：{counts}");

                    Logger.Info("", sb.ToString());
                  
                    List<HLYUser> list
                        = JsonConvert.DeserializeObject<List<HLYUser>>(result);

                    if (list.Count<= 0)
                    {
                        continue;
                    }

                    //获取所有组织
                    List<string> orgNumbers = GetAllOrgNumber();

                    //创建员工
                    for (int j = 0; j < list.Count; j++)
                    {
                        InterfaceModel interfaceModel = new InterfaceModel();
                        HLYUser user = list[j];

                        try
                        {
                            sb = new StringBuilder();
                            sb.AppendLine("");
                            sb.AppendLine($@"员工API处理数据:第{j+1}个,{user.fullName}");

                            #region 检查数据是否合法
                            string custNo = "";
                            string kingdeeCostCenter = "";
                            List<customFormValues> customFormValues = user.customFormValues;
                            if (customFormValues != null)
                            {
                                foreach (var item in customFormValues)
                                {
                                    string filedCode = item.fieldCode;
                                    if (filedCode == "kingdeeNo")
                                    {
                                        custNo = item.value;
                                    }
                                    if (filedCode == "KingcostCentercode01")
                                    {
                                        kingdeeCostCenter = item.value;
                                    }
                                }
                            }

                            if (string.IsNullOrWhiteSpace(custNo))
                            {
                                throw new Exception($@"工号为{user.employeeID}的金蝶员工工号为空");
                            }

                            if (string.IsNullOrWhiteSpace(user.dutyCode))
                            {
                                throw new Exception($@"该员工没有岗位，不新增");
                            }
                            #endregion

                            #region 获取部门编码
                            sb.AppendLine($@"开始获取部门:{user.custDeptNumber}");

                            string deptNo = SqlHelper.GetDeptNo(_ctx, user.custDeptNumber);
                            if (string.IsNullOrWhiteSpace(deptNo))
                            {
                                throw new Exception($@"名称为{user.departmentName}的部门不存在!");
                            }
                            #endregion

                            #region 岗位处理

                            string dutyCode = user.dutyCode;

                            

                            sb.AppendLine($@"判断是否需要新增岗位:{dutyCode}-{deptNo}");

                            dutyCode = SqlHelper.GetPostNo(_ctx, dutyCode);

                            sb.AppendLine($@"金蝶中获取到的岗位编码:{dutyCode}");

                            if (string.IsNullOrWhiteSpace(dutyCode))
                            {
                                dutyCode = user.dutyCode;

                                sb.AppendLine($@"开始创建岗位:{dutyCode}");

                                KingdeePostModel postModel = new KingdeePostModel();
                                postModel.FCreateOrgId = new BaseModel { FNumber = "100" };
                                postModel.FUseOrgId = new BaseModel { FNumber = "100" };
                                postModel.FName = user.duty;
                                postModel.FNumber = dutyCode;
                                postModel.FDept = new BaseModel { FNumber = deptNo };

                                interfaceModel = new InterfaceModel() { Model = postModel };
                                formId = "HR_ORG_HRPOST";
                                content = JsonHelper.ToJSON(interfaceModel);

                                sb.AppendLine($@"金蝶岗位请求信息:{content}");

                                response =
                                    KingdeeInterfaceUtil.Sync(formId, content, dutyCode, _dbid,false);

                                sb.AppendLine($@"金蝶岗位返回信息:{content}");

                                if (!string.IsNullOrWhiteSpace(response))
                                {
                                    throw new Exception($@"创建岗位出错：{response}");
                                }
                            }
                            else
                            {
                                //判断岗位是否需要修改
                                if (string.IsNullOrWhiteSpace(SqlHelper.GetPostDeptNo(_ctx, dutyCode, deptNo)))
                                {
                                    sb.AppendLine($@"开始修改岗位:{dutyCode}-{deptNo}");
                                    SqlHelper.UpdatePostDept(_ctx, dutyCode, deptNo);
                                }
                            }

                            #endregion

                            #region 查询员工是否存在，若存在按禁用，反禁用处理
                            sb.AppendLine($@"判断员工是否存在:{custNo}");

                            string id = SqlHelper.GetEmpId(_ctx, custNo);
                            //string id = "0";
                            if (id != "0")
                            {
                                if (user.status == "1002")
                                {
                                    int count = SqlHelper.ForbidEmp(_ctx, custNo);
                                    if (count > 0)
                                    {
                                        sb.AppendLine($@"禁用员工【{custNo}】");
                                    }

                                }

                                if (user.status == "1001")
                                {
                                    int count = SqlHelper.UnForbidEmp(_ctx, custNo ,kingdeeCostCenter);
                                    if (count > 0)
                                    {
                                        sb.AppendLine($@"反禁用员工【{custNo}】");
                                    }
                                }
                            }
                            #endregion

                            #region 查询员工是否存在，若不存在,创建员工
                            if (id == "0")
                            {
                                #region 创建员工
                                sb.AppendLine($@"开始创建员工:{custNo}");

                                KingdeeEmployeeModel model = new KingdeeEmployeeModel();
                                //model.FID = Convert.ToInt32(id);
                                model.FCreateOrgId = new BaseModel { FNumber = "100" };
                                model.FUseOrgId = new BaseModel { FNumber = "100" };
                                model.FStaffNumber = custNo;
                                model.FName = user.fullName;
                                model.F_PAEZ_Text5 = user.companyCode;
                                model.F_PAEZ_Base1 = new BaseModel { FNumber = kingdeeCostCenter };

                                //FPostEntity post = new FPostEntity();
                                //post.FPost = new BaseModel { FNumber = dutyCode };
                                //post.FIsFirstPost = "true";
                                //model.FPostEntity = new List<FPostEntity>() { post };

                                interfaceModel = new InterfaceModel() { Model = model };
                                formId = "BD_Empinfo";
                                content = JsonHelper.ToJSON(interfaceModel);

                                sb.AppendLine($@"金蝶员工请求信息:{content}");

                                response =
                                    KingdeeInterfaceUtil.Sync(formId, content, custNo, _dbid);

                                sb.AppendLine($@"金蝶员工返回信息:{response}");

                                if (!string.IsNullOrWhiteSpace(response))
                                {
                                    throw new Exception($@"创建员工出错：{response}");
                                }
                                #endregion
                            }
                            #endregion

                            #region 判断员工下当前岗位是否存在，若不存在，创建员工任岗明细/业务员
                            sb.AppendLine($@"判断是否需要新增员工任岗明细:{dutyCode}-{custNo}-{deptNo}");
                            if (string.IsNullOrWhiteSpace(SqlHelper.GetEmpPostNo(_ctx, dutyCode, custNo)))
                            {
                                #region 禁用之前的员工岗位信息
                                sb.AppendLine($@"禁用之前的任岗信息【{custNo}】");
                                SqlHelper.ForbidEmpPostNo(_ctx, custNo);
                                #endregion

                                #region 创建员工任岗明细
                                string staffNumber = "GW" + DateTime.Now.ToString("yyyyMMddHHmmss");
                                sb.AppendLine($@"开始创建员工任岗明细:{custNo}");

                                KingdeeStaffModel staffModel = new KingdeeStaffModel();
                                staffModel.FCreateOrgId = new BaseModel { FNumber = "100" };
                                staffModel.FUseOrgId = new BaseModel { FNumber = "100" };
                                staffModel.FEmpInfoId = new BaseModel { FNumber = custNo };
                                staffModel.FPosition = new BaseModel { FNumber = dutyCode };
                                staffModel.FStaffNumber = custNo;

                                FPOSTBILLEntity fPOSTBILLEntity = new FPOSTBILLEntity();
                                fPOSTBILLEntity.FIsFirstPost = "true";
                                staffModel.FPOSTBILLEntity = fPOSTBILLEntity;


                                interfaceModel = new InterfaceModel() { Model = staffModel };
                                formId = "BD_NEWSTAFF";
                                content = JsonHelper.ToJSON(interfaceModel);

                                sb.AppendLine($@"金蝶员工任岗明细请求信息:{content}");

                                response =
                                    KingdeeInterfaceUtil.Sync(formId, content, custNo, _dbid);

                                sb.AppendLine($@"金蝶员工任岗明细返回信息:{content}");

                                if (!string.IsNullOrWhiteSpace(response))
                                {
                                    sb.AppendLine($@"错误信息：创建员工任岗明细出错：{ response}");
                                    //throw new Exception($@"创建员工任岗明细出错：{response}");
                                }
                                #endregion

                                //#region 删除之前的业务员
                                //sb.AppendLine($@"开始删除业务员【{custNo}】");
                                //SqlHelper.DeleteOperator(_ctx, custNo);
                                //#endregion

                                #region 创建业务员
                                sb.AppendLine($@"开始创建业务员:{user.employeeID}");

                                KingdeeYWYModel xsy = new KingdeeYWYModel();
                                xsy.FOperatorType = "XSY";
                                KingdeeYWYModel cgy = new KingdeeYWYModel();
                                cgy.FOperatorType = "CGY";
                                List<YwyEntity> xsyEntity = new List<YwyEntity>();
                                List<YwyEntity> cgyEntity = new List<YwyEntity>();

                                foreach (var orgNumber in orgNumbers)
                                {
                                    YwyEntity xsyEntry = new YwyEntity();
                                    xsyEntry.FOperatorType_ETY = "XSY";
                                    xsyEntry.FBizOrgId = new BaseModel { FNumber = orgNumber };
                                    xsyEntry.FStaffId = new FStaffId { FNumber = custNo };
                                    xsyEntity.Add(xsyEntry);

                                    YwyEntity cgyEntry = new YwyEntity();
                                    cgyEntry.FOperatorType_ETY = "CGY";
                                    cgyEntry.FBizOrgId = new BaseModel { FNumber = orgNumber };
                                    cgyEntry.FStaffId = new FStaffId { FNumber = custNo };
                                    cgyEntity.Add(cgyEntry);
                                }
                                xsy.FEntity = xsyEntity;
                                cgy.FEntity = cgyEntity;

                                interfaceModel = new InterfaceModel() { Model = xsy };
                                formId = "BD_OPERATOR";
                                content = JsonHelper.ToJSON(interfaceModel);

                                sb.AppendLine($@"金蝶销售员请求信息:{content}");

                                response =
                                    KingdeeInterfaceUtil.Sync(formId, content, custNo, _dbid, false);

                                sb.AppendLine($@"金蝶销售员返回信息:{response}");

                                if (!string.IsNullOrWhiteSpace(response))
                                {
                                    throw new Exception($@"创建销售员出错：{response}");
                                }

                                interfaceModel = new InterfaceModel() { Model = cgy };
                                content = JsonHelper.ToJSON(interfaceModel);

                                sb.AppendLine($@"金蝶采购员请求信息:{content}");

                                response = KingdeeInterfaceUtil.Sync(formId, content, custNo, _dbid, false);

                                sb.AppendLine($@"金蝶采购员返回信息:{response}");

                                if (!string.IsNullOrWhiteSpace(response))
                                {
                                    throw new Exception($@"创建采购员出错：{response}");
                                }
                                #endregion 
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($@"错误信息：{ex.Message}");
                        }
                        finally
                        {
                            Logger.Info("", sb.ToString());
                        }
                    }

                }
                catch (Exception ex)
                {
                    sb.AppendLine($@"错误信息：{ex.Message}");
                }

                finally
                {
                    Logger.Info("", sb.ToString());
                }

            }
        }

        public void GetProgram()
        {
            int pages = 1;
            for (int i = 1; i <= pages; i++)
            {
                string token = GetToken();
                string url = $@"https://api.huilianyi.com/gateway/api/open/vendor";
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("access_token", token);
                dic.Add("startDate", DateTime.Now.AddDays(-100).ToString("yyyy-MM-dd HH:mm:ss"));
                dic.Add("endDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                dic.Add("page", i);
                dic.Add("size", 50);

                string result = ApiHelper.HttpGet(url, dic, out pages);

                List<HLYSupplier> list
                    = JsonConvert.DeserializeObject<List<HLYSupplier>>(result);

            }
        }

        public string GetToken()
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

        public List<string> GetAllOrgNumber()
        {
            string sql = $@"
                SELECT  A.FNumber
                  FROM  T_ORG_ORGANIZATIONS A  
                 WHERE  A.FDOCUMENTSTATUS = 'C'
                   AND  A.FNUMBER <> '100'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(_ctx, sql);

            if (data.Count <= 0)
            {
                return new List<string>();
            }

            return data.Select(x => x["FNumber"].ToString()).ToList();
        }
    }
}
