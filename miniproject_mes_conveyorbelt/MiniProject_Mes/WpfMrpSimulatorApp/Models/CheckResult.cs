using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMrpSimulatorApp.Models
{   //json 전송용 객체
    public class CheckResult
    {
        public int PIdx { get; set; }

        public string TimeStamp { get; set; }

        public string Result {  get; set; } 

        public int LoadTime { get; set; }
        public string PlantCode { get; set; }
    }
}
