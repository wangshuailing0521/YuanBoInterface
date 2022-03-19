using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class KingdeeSubmitResponse
    {
        public KingdeeSubmitResult Result { get; set; }
    }

    public class KingdeeSubmitResponseResult
    {
        /// <summary>
        /// 
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<NeedReturnDataItem> NeedReturnData { get; set; }
    }

    public class KingdeeSubmitResult
    {
        /// <summary>
        /// 
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }

    }
}
