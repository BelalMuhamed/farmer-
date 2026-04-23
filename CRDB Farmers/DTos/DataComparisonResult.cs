using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDB_Farmers.DTos
{
    //internal class DataComparisonResult
    //{
    //    public List<failedRecords> OnlyInOrFailedFromEmbossingFile { get; set; } = new List<failedRecords>();
    //    public List<failedRecords> OnlyInOrFailedFromExcelFile { get; set; } = new List<failedRecords>();

    //    public Dictionary<string, FinalResultDto> CommonData { get; set; }
    //        = new Dictionary<string, FinalResultDto>();

    //    public void AddEmbossingRecord(string accountNumber, string reason)
    //    {
    //        OnlyInOrFailedFromEmbossingFile.Add(new failedRecords
    //        {
    //            AccountNumber = accountNumber,
    //            Reason = reason,
    //            From = "EmbossingFile"
    //        });
    //    }

    //    public void AddExcelRecord(string accountNumber, string reason)
    //    {
    //        OnlyInOrFailedFromExcelFile.Add(new failedRecords
    //        {
    //            AccountNumber = accountNumber,
    //            Reason = reason,
    //            From = "ExcelFile"
    //        });
    //    }
    //}
    internal class DataComparisonResult
    {
        public List<RepeatedAccountReportDto> RepeatedAccounts { get; set; }


        public List<failedRecords> OnlyInOrFailedFromEmbossingFile { get; set; } = new List<failedRecords>();
        public List<failedRecords> OnlyInOrFailedFromExcelFile { get; set; } = new List<failedRecords>();

        // المرحلة 1: matched فقط
        public List<FinalResultDto> MatchedData { get; set; } = new List<FinalResultDto>();

        // المرحلة 2: final valid after images
        public List<FinalResultDto> ValidData { get; set; } = new List<FinalResultDto>();

        public void AddEmbossingRecord(string accountNumber, string reason)
        {
            OnlyInOrFailedFromEmbossingFile.Add(new failedRecords
            {
                AccountNumber = accountNumber,
                Reason = reason,
                From = "EmbossingFile"
            });
        }

        public void AddExcelRecord(string accountNumber, string reason)
        {
            OnlyInOrFailedFromExcelFile.Add(new failedRecords
            {
                AccountNumber = accountNumber,
                Reason = reason,
                From = "ExcelFile"
            });
        }
    }
}
