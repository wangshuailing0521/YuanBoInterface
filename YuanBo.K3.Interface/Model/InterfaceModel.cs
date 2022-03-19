using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class InterfaceModel
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> NeedUpDateFields { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> NeedReturnFields { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IsDeleteEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SubSystemId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IsVerifyBaseDataField { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IsEntryBatchFill { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ValidateFlag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string NumberSearch { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InterationFlags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAutoSubmitAndAudit = true;
        /// <summary>
        /// 
        /// </summary>
        public object Model { get; set; }

    }
}
