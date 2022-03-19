using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Transactions;

using YuanBo.K3.Interface.Helper;
using YuanBo.K3.Interface.Model;

using static Kingdee.BOS.Core.Metadata.EntityElement.TaxDetailSubEntryEntity;

namespace YuanBo.K3.Interface.Schedule.FK
{
    [Description("供应商自动审核")]
    public class SupplierAutoAudit: IScheduleService
    {
        Context _ctx;
        public void Run(Context ctx, Kingdee.BOS.Core.Schedule schedule)
        {
            _ctx = ctx;

            AutoAudit();
        }

        void AutoAudit()
        {
            DynamicObjectCollection data = GetData();
            if (data.Count <= 0)
            {
                return;
            }

            foreach (var item in data)
            {
                FormMetadata formMetadata = ServiceHelper.GetService<IMetaDataService>().Load(_ctx, "AR_receivable") as FormMetadata;
                BusinessInfo businessInfo = formMetadata.BusinessInfo;

                List<KeyValuePair<object, object>> pkIds = new List<KeyValuePair<object, object>>();
                pkIds.Add(new KeyValuePair<object, object>(item["FSupplierID"].ToString(), ""));

                List<object> list = new List<object>();
                foreach (KeyValuePair<object, object> pkId in pkIds)
                {
                    list.Add(Convert.ToInt64(pkId.Key));
                }

                IOperationResult operationResult 
                    = ServiceHelper.GetService<ISubmitService>().Submit(_ctx, businessInfo, list.ToArray(), "Submit");
                List<object> list2 = new List<object>();
                list2.Add("1");
                list2.Add("");
                IOperationResult operationResult2 
                    = ServiceHelper.GetService<ISetStatusService>().SetBillStatus(_ctx, businessInfo, pkIds, list2, "Audit", OperateOption.Create());
            
        }
        }

        private DynamicObjectCollection GetData()
        {
            string sql = $@"
                SELECT  FSupplierID
                  FROM  T_BD_Supplier 
                 WHERE  FDocumentStatus = 'A'";
            DynamicObjectCollection data
                = DBUtils.ExecuteDynamicObject(_ctx, sql);
            return data;
        }
    }
}
