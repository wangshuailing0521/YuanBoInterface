using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class FeiKongResponse
    {
        public string message { get; set; }
        public string errorCode { get; set; }
        public string oid { get; set; }
        public string key { get; set; }
    }

    public class CostCenterItemReponse
    {
        public string costCenterItemOID { get; set; }
        public string code { get; set; }
    }
}
