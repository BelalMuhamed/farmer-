using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CRDB_Farmers.DTos
{
    internal class failedRecords
    {
        public string AccountNumber { get; set; }
        public string Reason { get; set; }
        public string  From { get; set; }
    }
}
