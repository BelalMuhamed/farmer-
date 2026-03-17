using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDB_Farmers.DTos
{
    internal class DataBaseConfigModel
    {
        public string DataBaseName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;

        public string Table_fields { get; set; } = string.Empty;
        public int UNIQUE { get; set; }
        public Dictionary<string, string> FieldTypes { get; set; } = new Dictionary<string, string>(); 
    }
}
