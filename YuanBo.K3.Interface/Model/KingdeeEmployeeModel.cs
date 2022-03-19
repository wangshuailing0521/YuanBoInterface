using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class KingdeeEmployeeModel
    {
        public int FID { get; set; }

        public BaseModel FCreateOrgId { get; set; }

        public BaseModel FUseOrgId { get; set; }

        public string FStaffNumber { get; set; }

        public string FName { get; set; }

        public string F_PAEZ_Text5 { get; set; }

        public BaseModel F_PAEZ_Base1 { get; set; }

        public List<FPostEntity> FPostEntity { get; set; }
    }

    public class FPostEntity
    {
        public BaseModel FPost { get; set; }

        public string FIsFirstPost = "false";
    }
}
