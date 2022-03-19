using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class KingdeeSaveResponse
    {
        public KingdeeResponseResult Result { get; set; }
    }

    public class SuccessEntitysItem
    {
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
        public int DIndex { get; set; }
    }

    public class Errors
    {
        public string FieldName { get; set; }

        public string Message { get; set; }

        public string Dindex { get; set; }
    }
        

    public class ResponseStatus
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<Errors> Errors { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<SuccessEntitysItem> SuccessEntitys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> SuccessMessages { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int MsgCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int ErrorCode { get; set; }
    }

    public class NeedReturnDataItem
    {
    }

    public class KingdeeResponseResult
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
}
