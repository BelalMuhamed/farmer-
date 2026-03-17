using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDB_Farmers.DTos
{
    internal class ExcelDto
    {
        public string SN { get; set; }
        [DisplayName("farmer number")]
        public string FarmerNumber { get; set; }
        [DisplayName("account number")]
        public string AccountNumber { get; set; }
        [DisplayName("image")]
        public string FarmerImage { get; set; }
        [DisplayName("qr code")]
        public string QRCode { get; set; }
        [DisplayName("full name")]
        public string Name { get; set; }
    }
    internal class csvDto
    {
        [DisplayName("farmer number")]
        public string FarmerNumber { get; set; }
        [DisplayName("account number")]
        public string AccountNumber { get; set; }
        [DisplayName("image")]
        public string FarmerImage { get; set; }
        [DisplayName("qr code")]
        public string QRCode { get; set; }
        [DisplayName("full name")]
        public string Name { get; set; }
    }
}
