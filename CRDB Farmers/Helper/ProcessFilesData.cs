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
            // ================================
            // 1️⃣ Detect repeated accounts
            // ================================
            var repeatedAccounts = embossingData
                .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
                .GroupBy(x => x.AccountNumber)
                .Where(g => g.Count() > 1)
                .Select(g => new RepeatedAccountReportDto
                {
                    AccountNumber = g.Key,
                    Source = "Embossing",
                    Count = g.Count()
                })
                .ToList();

            repeatedAccounts.AddRange(
                excelData
                    .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
                    .GroupBy(x => x.AccountNumber)
                    .Where(g => g.Count() > 1)
                    .Select(g => new RepeatedAccountReportDto
                    {
                        AccountNumber = g.Key,
                        Source = "ExcelFile",
                        Count = g.Count()
                    })
            );

            result.RepeatedAccounts = repeatedAccounts;

            // ================================
            // 2️⃣ Block repeated accounts
            // ================================
            var blockedAccounts = repeatedAccounts
                .Select(x => x.AccountNumber)
                .ToHashSet();

            // ================================
            // 3️⃣ Clean data (exclude repeated)
            // ================================
            var cleanEmbossing = embossingData
                .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
                .Where(x => !blockedAccounts.Contains(x.AccountNumber))
                .GroupBy(x => x.AccountNumber)
                .ToDictionary(g => g.Key, g => g.First());

            var cleanExcel = excelData
                .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
                .Where(x => !blockedAccounts.Contains(x.AccountNumber))
                .GroupBy(x => x.AccountNumber)
                .ToDictionary(g => g.Key, g => g.First());

            // ================================
            // 4️⃣ Matching
            // ================================
            foreach (var excel in cleanExcel.Values)
            {
                if (cleanEmbossing.TryGetValue(excel.AccountNumber, out var embossing))
                {
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
                    result.AddExcelRecord(
                        excel.AccountNumber,
                        "Not matched with embossing file");
                }
            }

            // ================================
            // 5️⃣ Embossing leftovers
            // ================================
            foreach (var kv in cleanEmbossing)
            {
                if (cleanExcel.ContainsKey(kv.Key))
                    continue;

                result.AddEmbossingRecord(
                    kv.Key,
                    "Not matched with excel file");
            }
        }
    }
}


