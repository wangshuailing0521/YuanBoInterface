using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanBo.K3.Interface.Schedule.FK;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            GetSupplierSchedule getSupplierSchedule = new GetSupplierSchedule();
            getSupplierSchedule.GetUser();
        }
    }
}
