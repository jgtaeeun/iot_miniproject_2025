using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMrpSimulatorApp.Models
{
    public class TotalConfig
    {
        public DatabaseConfig Database { get; set; }
       
    }

 
   
    public class DatabaseConfig
    {
        public string Server {  get; set; }
        public string Database { get; set; }
        public string User { get; set; }

        public string Password { get; set; }    
    }
}
