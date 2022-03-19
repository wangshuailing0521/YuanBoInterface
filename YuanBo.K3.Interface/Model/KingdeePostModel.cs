using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class KingdeePostModel
    {
        public int FPOSTID { get; set; }

        public BaseModel FCreateOrgId { get; set; }

        public BaseModel FUseOrgId { get; set; }

        public string FNumber { get; set; }

        public string FName { get; set; }

        public BaseModel FDept { get; set; }
    }
}
