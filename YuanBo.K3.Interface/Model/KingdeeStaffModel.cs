using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class KingdeeStaffModel
    {
        public BaseModel FCreateOrgId { get; set; }

        public BaseModel FUseOrgId { get; set; }

        public BaseModel FEmpInfoId { get; set; }

        //public BaseModel FDept { get; set; }

        public BaseModel FPosition { get; set; }

        public string FStaffNumber { get; set; }

        public FPOSTBILLEntity FPOSTBILLEntity { get; set; }
    }

    public class FPOSTBILLEntity
    {
        public string FIsFirstPost = "false";
    }
}
