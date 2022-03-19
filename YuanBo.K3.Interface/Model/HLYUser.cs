using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class HLYUser
    {
        public string employeeID { get; set; }

        public string fullName { get; set; }

        public string companyName { get; set; }

        public string companyCode { get; set; }

        public string departmentOID { get; set; }

        public string custDeptNumber { get; set; }

        public string departmentName { get; set; }

        public string duty { get; set; }

        public string dutyCode { get; set; }

        public string status { get; set; }

        public string title { get; set; }
        public List<customFormValues> customFormValues { get; set; }
    }

    public class customFormValues
    {
        public string fieldName { get; set; }

        public string value { get; set; }

        public string fieldCode { get; set; }
    }
}
