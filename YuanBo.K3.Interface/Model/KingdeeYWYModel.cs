using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class KingdeeYWYModel
    {
        public string FOperatorType { get; set; }

        public List<YwyEntity> FEntity { get; set; }
    }

    public class YwyEntity
    {
        public string FOperatorType_ETY { get; set; }

        public BaseModel FBizOrgId { get; set; }

        public FStaffId FStaffId { get; set; }
    }

    public class FStaffId
    {
        public string FNumber { get; set; }
    }
}
