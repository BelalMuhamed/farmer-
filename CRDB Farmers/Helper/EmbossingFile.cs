
using CRDB_Farmers.DTos;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDB_Farmers.Helper
{
    internal static class EmbossingFile
    {
        
        public static List<byte[]> FileRecordsDataInBytes { get; set; }
        public static string[] GetEmbossingFileData(string filePath)
        {
            if (!File.Exists(filePath))
                return null;
            var DelimeterInBytes=Encoding.ASCII.GetBytes("#END#");
            var allBytes = File.ReadAllBytes(filePath);
            FileRecordsDataInBytes = SplitRecords(allBytes, DelimeterInBytes);
            string fileContent = Encoding.Default.GetString(allBytes); 


            string[] records = fileContent.Split(
                new string[] { "#END#" },
                StringSplitOptions.RemoveEmptyEntries
            );

            return records;
        }

        private static string ExtractLine(string text, string prefix, int length)
        {
            var index = text.IndexOf(prefix);
            if (index == -1) return null;

            var start = index + prefix.Length;
            if (start + length > text.Length) return null;

            return text.Substring(start, length).Trim();
        }

        private static string ExtractBetween(string text, string startDelim, string endDelim)
        {
            var start = text.IndexOf(startDelim);
            if (start == -1) return null;

            start += startDelim.Length;

            if (endDelim == null)
                return text.Substring(start).Trim();

            var end = text.IndexOf(endDelim, start);
            if (end == -1) return null;

            return text.Substring(start, end - start).Trim();
        }
       
        private static List<byte[]> SplitRecords(byte[] fileBytes, byte[] delimiter)
        {
            var records = new List<byte[]>();
            int start = 0;

            for (int i = 0; i <= fileBytes.Length - delimiter.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < delimiter.Length; j++)
                {
                    if (fileBytes[i + j] != delimiter[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    int length = i - start;
                    if (length > 0)
                    {
                        byte[] record = new byte[length];
                        Array.Copy(fileBytes, start, record, 0, length);
                        records.Add(record);
                    }
                    start = i + delimiter.Length;
                    i = start - 1; // continue after the delimiter
                }
            }

            // Add last record if exists (in case no trailing delimiter)
            if (start < fileBytes.Length)
            {
                byte[] record = new byte[fileBytes.Length - start];
                Array.Copy(fileBytes, start, record, 0, record.Length);
                records.Add(record);
            }

            return records;
        }

        private static byte[] ExtractRawChipData(byte[] recordBytes)
        {
            int braceIndex = Array.IndexOf(recordBytes, (byte)'{');
            if (braceIndex == -1 || braceIndex + 8 > recordBytes.Length)
                return Array.Empty<byte>();

            int startIndex = braceIndex + 1;

            string lengthText = Encoding.ASCII.GetString(recordBytes, startIndex, 7);
            if (!int.TryParse(lengthText, out int byteCount))
                throw new Exception("Invalid length format in chip data");

            int dataStartIndex = startIndex + 7;

            if (dataStartIndex + byteCount > recordBytes.Length)
                throw new Exception("Chip data length exceeds record size");

            byte[] chipData = new byte[byteCount];
            Array.Copy(recordBytes, dataStartIndex, chipData, 0, byteCount);

            return chipData;
        }

        public static List<EmbossingDto> Parse(string[] records, List<byte[]> recordsInBytes , ref DataComparisonResult _result)
        {
            
                var list = new List<EmbossingDto>();
                        for(int i = 0;i< records.Length;i++)
                        {
                             EmbossingDto dto=new EmbossingDto();
                                try
                                {

                                        dto.PAN = ExtractLine(records[i], "Line1 ", 19);
                                        dto.EXP = ExtractLine(records[i], "Line2 ", 5);
                                        dto.CVV2 = ExtractLine(records[i], "Line5 ", 8);
                                        dto.Track1 = ExtractBetween(records[i], "%", "?");
                                        dto.Track2 = ExtractBetween(records[i], ";", "?");
                                        dto.AccountNumber = ExtractLine(records[i], "Line4 ", 14)?.Replace(" ", "");
                                        dto.NAME= ExtractBetween(records[i], "Line3 ", "Line4");
                                        dto.chip = ExtractRawChipData(recordsInBytes[i]);

                                        
                                   
                                }
                                catch (Exception ex) 
                                {
                                _result.AddEmbossingRecord(dto.AccountNumber, $"cannot process the data because {ex.Message}");
                                 continue; 
                                }
                                
                                list.Add(dto);
                                

                }
            return list;

        }

       


    }
    }

