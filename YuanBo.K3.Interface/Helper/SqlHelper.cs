using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanBo.K3.Interface.Model;

namespace YuanBo.K3.Interface.Helper
{
    public static class SqlHelper
    {
        public static void UpdateDept(Context context, string deptId, string crmId)
        {
            string sql = $@"
                UPDATE  T_BD_DEPARTMENT
                   SET  FCRMId='{crmId}'
                 WHERE  FDEPTID='{deptId}'";
            DBUtils.Execute(context, sql);
        }

        public static void UpdateAssistant(Context context, string entryId, string oid)
        {
            string sql = $@"
                UPDATE  T_BAS_ASSISTANTDATAENTRY
                   SET  FOID='{oid}'
                 WHERE  FENTRYID ='{entryId}'";
            DBUtils.Execute(context, sql);
        }

        public static string GetHlyDeptNo(Context context, string name)
        {
            string sql = $@"
                SELECT  A.FNUMBER
                  FROM T_BD_DEPARTGROUP A
                       INNER JOIN T_BD_DEPARTGROUP_L B ON A.FID = B.FID                   
                 WHERE  B.FName LIKE '%{name}%'
                   AND  A.F_PAEZ_INTEGER = 1";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FNUMBER"].ToString();
        }

        public static string GetDeptNo(Context context, string no)
        {
            string sql = $@"
                SELECT  A.FNUMBER
                  FROM T_BD_DEPARTMENT A
                       INNER JOIN T_BD_DEPARTMENT_L B ON A.FDEPTID = B.FDEPTID                   
                 WHERE  A.FNUMBER = '{no}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FNUMBER"].ToString();
        }

        /// <summary>
        /// 查找员工任岗明细
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="deptCode"></param>
        /// <returns></returns>
        public static string GetEmpPostNo(Context context, string dutyCode,string empNo)
        {
            string sql = $@"
                SELECT  A.FNUMBER
                  FROM  T_ORG_POST A WITH(NOLOCK)     
                        INNER JOIN T_BD_STAFF D WITH(NOLOCK) ON D.FPOSTID = A.FPOSTID
                        INNER JOIN T_HR_EMPINFO E WITH(NOLOCK) ON D.FEMPINFOID = E.FID
                 WHERE  A.FNUMBER = '{dutyCode}'
                   AND  E.FNUMBER = '{empNo}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FNUMBER"].ToString();
        }

        /// <summary>
        /// 禁用员工任岗明细
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="deptCode"></param>
        /// <returns></returns>
        public static string ForbidEmpPostNo(Context context, string empNo)
        {
            string sql = $@"/*dialect*/  
                UPDATE  D 
                   SET  D.FFORBIDSTATUS = 'B', 
                        D.FFORBIDDATE = GETDATE(),
                        D.FFORBIDDERID = 100006
                  FROM  T_ORG_POST A       
                        INNER JOIN T_BD_STAFF D ON D.FPOSTID = A.FPOSTID
                        INNER JOIN T_HR_EMPINFO E ON D.FEMPINFOID = E.FID
                 WHERE  1=1
                   AND  D.FFORBIDSTATUS = 'A' 
                   AND  E.FNUMBER = '{empNo}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FNUMBER"].ToString();
        }

        /// <summary>
        /// 删除业务员
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="deptCode"></param>
        /// <returns></returns>
        public static void DeleteOperator(Context context, string empNo)
        {
            string sql = $@"/*dialect*/   
                DELETE
                  FROM  T_BD_OPERATORENTRY      
                 WHERE  FSTAFFID IN (
                        SELECT  
                                FSTAFFID  
                          FROM  T_BD_STAFF 
                         WHERE  FSTAFFNUMBER = '{empNo}')
                        ";
                                   
             DBUtils.Execute(context, sql);
        }

        public static string GetPostNo(Context context, string dutyCode)
        {
            string sql = $@"
                SELECT  A.FNUMBER
                  FROM  T_ORG_POST A       
                        --INNER JOIN T_ORG_POST_L B ON A.FPOSTID =B.FPOSTID AND B.FLOCALEID = 2052
                 WHERE  A.FNUMBER = '{dutyCode}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FNUMBER"].ToString();
        }

        public static string GetPostDeptNo(Context context, string dutyCode,string deptNo)
        {
            string sql = $@"
                SELECT  A.FNUMBER
                  FROM  T_ORG_POST A       
                        INNER JOIN T_BD_DEPARTMENT B ON A.FDEPTID = B.FDEPTID
                 WHERE  A.FNUMBER = '{dutyCode}'
                   AND  B.FNumber = '{deptNo}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FNUMBER"].ToString();
        }

        public static void UpdatePostDept(Context context, string dutyCode, string deptNo)
        {
            string sql = $@"/*dialect*/ 
                UPDATE  A
                   SET  A.FDEPTID = (SELECT TOP 1 FDEPTID FROM T_BD_DEPARTMENT WHERE FNUMBER = '{deptNo}')
                  FROM  T_ORG_POST A       
                 WHERE  A.FNUMBER = '{dutyCode}'";
            DBUtils.Execute(context, sql);
        }

        public static void UpdateHlyDept(Context context, string name, string hlyNumber)
        {
            string sql = $@"/*dialect*/ 
                UPDATE  A
                   SET  A.FHLYNo='{hlyNumber}'
                  FROM T_BD_DEPARTMENT A
                       INNER JOIN T_BD_DEPARTMENT_L B ON A.FDEPTID = B.FDEPTID
                 WHERE  B.FName = '{name}'";
            DBUtils.Execute(context, sql);
        }

        public static void UpdateEmployee(Context context, string empId, string crmId)
        {
            string sql = $@"
                UPDATE  T_HR_EMPINFO
                   SET  FCRMId='{crmId}'
                 WHERE  FID='{empId}'";
            DBUtils.Execute(context, sql);
        }

        public static void UpdateStation(Context context, string postId, string crmId)
        {
            string sql = $@"
                /*dialect*/
                UPDATE  T_ORG_POST
                   SET  FCRMId='{crmId}'
                 WHERE  FPOSTID='{postId}'";
            DBUtils.Execute(context, sql);
        }

        public static InterfaceUrl GetCRMUrl(Context context, string deptId)
        {
            string sql = $@"
                SELECT  DISTINCT
                        FEmployeeUrl
                       ,FEmployeeStatusUrl
                       ,FDeptUrl
                       ,FStationUrl
                       ,FReceivableUrl
                       ,FDYUrl
                       ,FDYStatusUrl
                       ,FReceBillRStatusUrl
                       ,FReceBillStatusUrl
                       ,FReceivableFPUrl
                       ,FReceivableKDUrl
                       ,FHXUrl
                       ,FReceivablePlanUrl
                  FROM  T_BD_DEPARTMENT
                 WHERE  FDEPTID = {deptId}";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);
            if (data.Count <= 0)
            {
                throw new Exception($@"部门【{deptId}】中CRMUr获取失败");
            }

            InterfaceUrl interfaceUrl
                = new InterfaceUrl()
                {
                    EmployeeUrl = data[0]["FEmployeeUrl"].ToString(),
                    EmployeeStatusUrl = data[0]["FEmployeeStatusUrl"].ToString(),
                    DeptUrl = data[0]["FDeptUrl"].ToString(),
                    StationUrl = data[0]["FStationUrl"].ToString(),
                    ReceivableUrl = data[0]["FReceivableUrl"].ToString(),
                    DYUrl = data[0]["FDYUrl"].ToString(),
                    DYStatusUrl = data[0]["FDYStatusUrl"].ToString(),
                    ReceBillRStatusUrl = data[0]["FReceBillRStatusUrl"].ToString(),
                    ReceBillStatusUrl = data[0]["FReceBillStatusUrl"].ToString(),
                    FReceivableFPUrl = data[0]["FReceivableFPUrl"].ToString(),
                    FReceivableKDUrl = data[0]["FReceivableKDUrl"].ToString(),
                    HXUrl = data[0]["FHXUrl"].ToString(),
                    ReceivablePlanUrl = data[0]["FReceivablePlanUrl"].ToString()
                };

            return interfaceUrl;
        }

        public static DynamicObjectCollection GetYSInfoByDY(Context context, string fid)
        {
            string sql = $@"
                SELECT  E.FBILLNO,D.FENTRYID,D.F_PAEZ_CHECKBOX,D.FBILLSTATUS1
                  FROM  PAEZ_T_DYSY A
                        INNER JOIN PAEZ_T_DYSYEntry B ON A.FID=B.FID
                        INNER JOIN PAEZ_t_Cust_Entry100006_LK C ON C.FENTRYID = B.FENTRYID
                        INNER JOIN t_AR_receivableEntry D ON C.FSBILLID = D.FID AND C.FSID = D.FENTRYID
                        INNER JOIN t_AR_receivable E ON E.FID=D.FID
                 WHERE  A.FID = {fid}";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            return data;
        }

        public static string GetDYStatus(Context context, string fid)
        {
            string sql = $@"
                SELECT  A.FDOCUMENTSTATUS
                  FROM  PAEZ_T_DYSY A                        
                 WHERE  A.FID = {fid}";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count<=0)
            {
                return "";
            }

            return data[0]["FDOCUMENTSTATUS"].ToString();
        }

        public static string GetReceBillReturnStatus(Context context, string fid)
        {
            string sql = $@"
                SELECT  A.FDOCUMENTSTATUS
                  FROM  T_AR_REFUNDBILL A                        
                 WHERE  A.FID = {fid}";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FDOCUMENTSTATUS"].ToString();
        }

        public static string GetReceBillStatus(Context context, string fid)
        {
            string sql = $@"
                SELECT  A.FDOCUMENTSTATUS
                  FROM  T_AR_RECEIVEBILL A                        
                 WHERE  A.FID = {fid}";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "";
            }

            return data[0]["FDOCUMENTSTATUS"].ToString();
        }

        public static void EditYSInfoByDY(Context context, string fid)
        {
            string sql = $@"/*dialect*/
                UPDATE  D SET D.F_PAEZ_CHECKBOX = '1'
                  FROM  PAEZ_T_DYSY A
                        INNER JOIN PAEZ_T_DYSYEntry B ON A.FID=B.FID
                        INNER JOIN PAEZ_t_Cust_Entry100006_LK C ON C.FENTRYID = B.FENTRYID
                        INNER JOIN t_AR_receivableEntry D ON C.FSBILLID = D.FID AND C.FSID = D.FENTRYID
                        INNER JOIN t_AR_receivable E ON E.FID=D.FID
                 WHERE  A.FID = {fid}
                   AND  D.FBILLSTATUS1 = 'B'";

            DBUtils.Execute(context, sql);

            sql = $@"/*dialect*/
                UPDATE  D SET D.F_PAEZ_CHECKBOX = '0'
                  FROM  PAEZ_T_DYSY A
                        INNER JOIN PAEZ_T_DYSYEntry B ON A.FID=B.FID
                        INNER JOIN PAEZ_t_Cust_Entry100006_LK C ON C.FENTRYID = B.FENTRYID
                        INNER JOIN t_AR_receivableEntry D ON C.FSBILLID = D.FID AND C.FSID = D.FENTRYID
                        INNER JOIN t_AR_receivable E ON E.FID=D.FID
                 WHERE  A.FID = {fid}
                   AND  D.FBILLSTATUS1 = 'A'";

            DBUtils.Execute(context, sql);

        }

        public static bool HaveSupplier(Context context, string number)
        {
            string sql = $@"
                SELECT  A.FSUPPLIERID
                  FROM  T_BD_SUPPLIER A  
                 WHERE  A.FNUMBER = '{number}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return false;
            }

            return true;
        }

        public static string GetSupplierID(Context context, string number)
        {
            string sql = $@"
                SELECT  A.FSUPPLIERID
                  FROM  T_BD_SUPPLIER A  
                 WHERE  A.FNUMBER = '{number}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count > 0)
            {
                return data[0]["FSUPPLIERID"].ToString();
            }

            return "";
        }

        public static string GetSupplierBankInfo(Context context, string number,string bankAccount)
        {
            string sql = $@"
                SELECT  A.FSUPPLIERID
                  FROM  T_BD_SUPPLIER A  
                        INNER JOIN t_BD_SupplierBank B
                        ON A.FSupplierID = B.FSupplierID
                 WHERE  A.FNUMBER = '{number}'
                   and  A.FUSEORGID = 1
                   AND  B.FBANKCODE = '{bankAccount}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count > 0)
            {
                return data[0]["FSUPPLIERID"].ToString();
            }

            return "";
        }


        public static List<string> GetSupplierIds(Context context, string number)
        {
            string sql = $@"
                SELECT  A.FSUPPLIERID
                  FROM  T_BD_SUPPLIER A  
                 WHERE  A.FNUMBER = '{number}'
                   and  A.FUSEORGID <> 1";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count > 0)
            {
                return data.Select(x => x["FSUPPLIERID"].ToString()).ToList();
            }

            return null;
        }

        public static bool AddSupplierBankInfo(
            Context context,
            string supplierId,
            string entryId,
            string bankAccount,
            string venBankNumberName,
            string bankOpeningBank,
            string masterId = "0")
        {
            string sql = $@"/*dialect*/ 
                INSERT  INTO t_BD_SupplierBank(
                        FBANKID,FSUPPLIERID,FOPENBANKNAME,FBANKCODE,FBANKHOLDER,FMASTERID)
                SELECT  (
                        {entryId},{supplierId},'{bankOpeningBank}','{bankAccount}','{venBankNumberName}',{masterId}
                        )";

            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 禁用供应商
        /// </summary>
        /// <param name="context"></param>
        /// <param name="number"></param>
        public static int ForbidSupplier(Context context, string number)
        {
            string sql = $@"
                UPDATE  T_BD_SUPPLIER
                   SET  FFORBIDSTATUS = 'B', 
                        FFORBIDDATE = GETDATE(),
                        FFORBIDERID = 100006
                 WHERE  FNUMBER = '{number}'
                   AND  FFORBIDSTATUS = 'A'";

           return  DBUtils.Execute(context, sql);
        }

        /// <summary>
        /// 反禁用供应商
        /// </summary>
        /// <param name="context"></param>
        /// <param name="number"></param>
        public static int UnForbidSupplier(Context context, string number)
        {
            string sql = $@"
                UPDATE  T_BD_SUPPLIER
                   SET  FFORBIDSTATUS = 'A', 
                        FFORBIDDATE = NULL,
                        FFORBIDERID = 0
                 WHERE  FNUMBER = '{number}'
                   AND  FFORBIDSTATUS = 'B'";

            return DBUtils.Execute(context, sql);
        }

        /// <summary>
        /// 禁用员工
        /// </summary>
        /// <param name="context"></param>
        /// <param name="number"></param>
        public static int ForbidEmp(Context context, string number)
        {
            string sql = $@"
                UPDATE  T_HR_EMPINFO
                   SET  FFORBIDSTATUS = 'B', 
                        FFORBIDDATE = GETDATE(),
                        FFORBIDDERID = 100006
                 WHERE  FNUMBER = '{number}'
                   AND  FFORBIDSTATUS = 'A'";

            return DBUtils.Execute(context, sql);
        }

        /// <summary>
        /// 反禁用员工
        /// </summary>
        /// <param name="context"></param>
        /// <param name="number"></param>
        public static int UnForbidEmp(Context context, string number,string kingdeeCostCenter = "")
        {
            int deptId = 0;
            string sql = $@"SELECT FDEPTID FROM T_BD_DEPARTMENT WHERE FNUMBER = '{kingdeeCostCenter}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);
            if (data.Count > 0)
            {
                deptId = Convert.ToInt32(data[0]["FDEPTID"]);
            }

            sql = $@"
                UPDATE  T_HR_EMPINFO
                   SET  FFORBIDSTATUS = 'A', 
                        FFORBIDDATE = NULL,
                        FFORBIDDERID = 0,
                        F_PAEZ_Base1 = {deptId}
                 WHERE  FNUMBER = '{number}'
                   --AND  FFORBIDSTATUS = 'B'
";

            return DBUtils.Execute(context, sql);
        }
        public static string GetEmpId(Context context, string number)
        {
            string sql = $@"
                SELECT  A.FID
                  FROM  T_HR_EMPINFO A 
                 WHERE  A.FNUMBER = '{number}'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(context, sql);

            if (data.Count <= 0)
            {
                return "0";
            }

            return data[0]["FID"].ToString();
        }

        public static int UpdateSupplierGroup(Context context, string no,string supplierNo)
        {
            string sql = $@"/*dialect*/ 
                UPDATE  A
                   SET  A.FPRIMARYGROUP= (SELECT TOP 1 FID 
                                     FROM T_BD_SUPPLIERGROUP 
                                    WHERE FNUMBER = '{no}')
                  FROM  T_BD_Supplier A
                 WHERE  FNumber = '{supplierNo}'
                ";

           return DBUtils.Execute(context, sql);
        }

        public static int UpdateSupplierType(Context context, string no, string supplierNo)
        {
            string sql = $@"/*dialect*/ 
                UPDATE  B
                   SET  B.FSupplierClassify= (SELECT TOP 1 FEntryID 
                                     FROM T_BAS_ASSISTANTDATAENTRY 
                                    WHERE FNUMBER = '{no}'
                                      AND FID = '442339bbee6c4f05b7f249e6d83ac9e2')
                  FROM  T_BD_Supplier A
                        INNER JOIN T_BD_SUPPLIERBASE B
						ON A.FSUPPLIERID = B.FSUPPLIERID
                 WHERE  FNumber = '{supplierNo}'
                ";

           return DBUtils.Execute(context, sql);
        }

    }
}
