using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMqttSubApp.Models
{   //json 전송용 객체
    public class CheckResult
    {
        public string ClientId { get; set; }    
        public string TimeStamp { get; set; }

        //양품,불량품 구분
        public string Result { get; set; }  
    }
}
