using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace CRDB_Farmers
{
    public static class Excel
    {

      
        public static List<T> ReadExcelAsList<T>(string filePath) where T : new()
        {
            var list = new List<T>();

            var lines = File.ReadAllLines(filePath);

            if (lines.Length <= 1)
                return list;

            var headers = lines[0]
       .Split(',')
       .Select(h => h.Trim().Trim('"').ToLower())
       .ToArray();

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var propMap = props.ToDictionary(
                p => p.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName?.Trim().ToLower() ?? p.Name.Trim().ToLower(),
                p => p
            );

            for (int row = 1; row < lines.Length; row++)
            {
                var values = lines[row]
       .Split(',')
       .Select(v => v.Trim().Trim('"'))
       .ToArray();

                T obj = new T();

                for (int i = 0; i < headers.Length && i < values.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(headers[i]))
                        continue;

                    if (propMap.TryGetValue(headers[i], out PropertyInfo prop))
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(values[i]))
                            {
                                object value = Convert.ChangeType(values[i], prop.PropertyType);

                                prop.SetValue(obj, value);
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                list.Add(obj);
            }

            return list;
        }
        public static bool ExportListToExcel<T>(List<T> dataList, string outputPath,string FileName, ref string errorMessage) where T : new()
        {
            outputPath = Path.Combine(outputPath, $"{FileName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xlsx");
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Data");

                    var properties = typeof(T).GetProperties();

                    // Header
                    for (int i = 0; i < properties.Length; i++)
                        worksheet.Cell(1, i + 1).Value = properties[i].Name;

                    // Rows
                    for (int row = 0; row < dataList.Count; row++)
                    {
                        for (int col = 0; col < properties.Length; col++)
                        {
                            var value = properties[col].GetValue(dataList[row], null);
                            worksheet.Cell(row + 2, col + 1).Value = value?.ToString() ?? string.Empty;

                        }
                    }

                    workbook.SaveAs(outputPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Export failed: {ex.Message}";
                return false;
            }
        }
    }
}
