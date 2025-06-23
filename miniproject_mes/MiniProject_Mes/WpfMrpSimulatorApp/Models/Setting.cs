using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMrpSimulatorApp.Models
{
    public class Setting
    {
        [Key]
        public string BasicCode { get; set; }

        [Required]  
        public string CodeName { get; set; }    
        public string? CodeDesc { get; set; }
        public string? ReDt { get; set; }
        public string? ModDt { get; set; } 

    }
}
