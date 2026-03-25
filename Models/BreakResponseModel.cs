using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BufeApp.Models
{
    public class BreakResponseModel
    {
        public string date { get; set; }
        public Break[] breaks { get; set; }
    }

    public class Break
    {
        public string start { get; set; }
        public string end { get; set; }
    }
}
