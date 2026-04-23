using CRDB_Farmers.DTos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace CRDB_Farmers.Helper
{
    internal static class ProcessFilesData
    {






      
        public static void ProcessFiles(
     List<EmbossingDto> embossingData,
     List<ExcelDto> excelData,
     ref DataComparisonResult result)
        {
            // 1️⃣ Normalize (remove duplicates safely)
            var cleanEmbossing = embossingData
                .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
                .GroupBy(x => x.AccountNumber)
                .ToDictionary(g => g.Key, g => g.ToList());

            var cleanExcel = excelData
                .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
                .GroupBy(x => x.AccountNumber)
                .ToDictionary(g => g.Key, g => g.First());
            // get repeated columns for report 
            var excelRepeated = excelData
       .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
       .GroupBy(x => x.AccountNumber)
       .Where(g => g.Count() > 1)
       .Select(g => new RepeatedAccountReportDto
       {
           AccountNumber = g.Key,
           Source = "CSV",
           Count = g.Count()
       });
            var embossRepeated = embossingData
      .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
      .GroupBy(x => x.AccountNumber)
      .Where(g => g.Count() > 1)
      .Select(g => new RepeatedAccountReportDto
      {
          AccountNumber = g.Key,
          Source = "Embossing",
          Count = g.Count()
      });
            result.RepeatedAccounts = excelRepeated
    .Concat(embossRepeated)
    .ToList();

            var matchedEmbossingKeys = new HashSet<string>();

            // 2️⃣ MATCHING
            foreach (var excel in cleanExcel.Values)
            {
                if (cleanEmbossing.TryGetValue(excel.AccountNumber, out var list) && list.Count > 0)
                {
                    var embossing = list[0];
                    list.RemoveAt(0);

                    matchedEmbossingKeys.Add(excel.AccountNumber);

                    result.MatchedData.Add(new FinalResultDto
                    {
                        PAN = embossing.PAN,
                        EXP = embossing.EXP,
                        NAME = embossing.NAME?.Trim(),
                        CVV2 = embossing.CVV2,
                        Track1 = embossing.Track1,
                        Track2 = embossing.Track2,
                        FarmerNumber = excel.FarmerNumber,
                        AccountNumber = embossing.AccountNumber,
                        chip = embossing.chip
                    });
                }
                else
                {
                    result.AddExcelRecord(excel.AccountNumber,
                        "Not matched with embossing file");
                }
            }

            // 3️⃣ Embossing leftovers (not matched at all)
            foreach (var kv in cleanEmbossing)
            {
                if (matchedEmbossingKeys.Contains(kv.Key))
                    continue;

                foreach (var embossing in kv.Value)
                {
                    result.AddEmbossingRecord(
                        embossing.AccountNumber,
                        "Not matched with excel file");
                }
            }

        }
    }
}


