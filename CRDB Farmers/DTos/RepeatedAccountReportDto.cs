using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDB_Farmers.DTos
{
    public class RepeatedAccountReportDto
    {
        public string AccountNumber { get; set; }
        public string Source { get; set; } 
        public int Count { get; set; }
    }
}
