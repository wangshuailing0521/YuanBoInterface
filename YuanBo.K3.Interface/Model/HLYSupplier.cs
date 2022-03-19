using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanBo.K3.Interface.Model
{
    public class HLYSupplier
    {
        public string vendorTypeCode { get; set; }

        public string vendorNumber { get; set; }

        public string vendorName { get; set; }

        public string status { get; set; }

        public List<BankAccounts> bankAccounts { get; set; }
    }

    public class BankAccounts
    {
        /// <summary>
        /// 银行账号
        /// </summary>
        public string bankAccount { get; set; }

        /// <summary>
        /// 开户银行
        /// </summary>
        public string bankOpeningBank { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        public string venBankNumberName { get; set; }
    }

}
