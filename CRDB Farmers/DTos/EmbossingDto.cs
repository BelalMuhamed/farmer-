using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDB_Farmers.DTos
{
    internal class EmbossingDto
    {
        public string PAN { get; set; }
        public string NAME { get; set; }
        public string EXP { get; set; }
        public string CVV2 { get; set; }
        public string Track1 { get; set; }
        public string Track2 { get; set; }
        public byte[] chip { get; set; }
        public string  AccountNumber { get; set; }

    }
}
