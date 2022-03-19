using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class KingdeeSupplierModel
    {
        public int FSupplierId { get; set; }

        public BaseModel FCreateOrgId { get; set; }

        public BaseModel FUseOrgId { get; set; }

        public string FNumber { get; set; }

        public string FName { get; set; }

        public BaseModel FGroup { get; set; }

        public FSupplierBase FBaseInfo { get; set; }

        public FSupplierBusinessInfo FBusinessInfo { get; set; }

        public FSupplierFFinanceInfo FFinanceInfo { get; set; }

        public List<FSupplierBank> FBankInfo { get; set; }
    }

    public class FSupplierBase
    {
        public BaseModel FSupplierClassify { get; set; }
    }

    public class FSupplierBusinessInfo
    {
        public BaseModel FSettleTypeId { get; set; }
    }

    public class FSupplierFFinanceInfo
    {
        public BaseModel FPayCurrencyId { get; set; }

        //public BaseModel FTaxType { get; set; }

        //public BaseModel FTaxRateId { get; set; }
    }

    public class FSupplierBank
    {
        public string FOpenBankName { get; set; }
        public string FBankCode { get; set; }
        public string FBankHolder { get; set; }
    }
}
