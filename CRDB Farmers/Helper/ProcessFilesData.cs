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
        //public static DataComparisonResult ProcessFiles(List<EmbossingDto> _embossingData, List<ExcelDto> _excelData)
        //{
        //    DataComparisonResult _result = new DataComparisonResult();

        //    for (int i = 0; i < _excelData.Count; i++)
        //    {
        //        var _embossingRecord = _embossingData.FirstOrDefault(e => e.AccountNumber == _excelData[i].AccountNumber);
        //        if (_embossingRecord != null)
        //        {
                    
        //            _result.CommonData.Add(new FinalResultDto
        //            {
        //                PAN = _embossingRecord.PAN,
        //                EXP = _embossingRecord.EXP,
        //                NAME = _excelData[i].Name,
        //                CVV2= _embossingRecord.CVV2,
        //                Track1= _embossingRecord.Track1,
        //                Track2= _embossingRecord.Track2,
        //                FarmerNumber=_excelData[i].FarmerNumber,
        //                AccountNumber = _embossingRecord.AccountNumber,
        //                chip= _embossingRecord.chip
        //            });
        //            _embossingData.Remove(_embossingRecord);
        //        }
        //        else
        //        {
                    
        //                if(!_result.CommonData.Any(c => c.AccountNumber== _excelData[i].AccountNumber))
        //                {
        //                    _result.OnlyInExcelFile.Add(_excelData[i]);

        //                }
                    
        //        }
               
        //    }
        //    if (_embossingData.Count > 0)
        //    {
        //        _result.OnlyInEmbossingFile = _embossingData;
        //    }
        //    return _result;
        //}


        public static void ProcessFiles(List<EmbossingDto> _embossingData, List<ExcelDto> _excelData, ref DataComparisonResult _result)
        {



            if (_embossingData.Count == _excelData.Count)
            {
                for (int i = 0; i < _embossingData.Count; i++)
                {
                    var _embossingRecord = _embossingData.FirstOrDefault(e => e.AccountNumber == _excelData[i].AccountNumber);
                    if (_embossingRecord != null)
                    {
                        try
                        {
                            _result.CommonData.Add(new FinalResultDto
                            {
                                PAN = _embossingRecord.PAN,
                                EXP = _embossingRecord.EXP,
                                NAME = _embossingRecord.NAME.Trim(),
                                CVV2 = _embossingRecord.CVV2,
                                Track1 = _embossingRecord.Track1,
                                Track2 = _embossingRecord.Track2,
                                FarmerNumber = _excelData[i].FarmerNumber,
                                AccountNumber = _embossingRecord.AccountNumber,
                                chip = _embossingRecord.chip
                            });

                        }
                        catch (Exception ex) 
                        {
                            _result.AddEmbossingRecord(_excelData[i].AccountNumber, $"Error in record data because {ex.Message} ");
                            continue;
                        }


                    }
                    else
                    {
                        _result.AddExcelRecord(_excelData[i].AccountNumber, "not matched with any record in embossing file ! ");
                       
                    }

                }
                for (int i = 0; i < _embossingData.Count; i++)
                {
                    var _embossingRecord = _result.CommonData.FirstOrDefault(e => e.AccountNumber == _embossingData[i].AccountNumber);
                    if (_embossingRecord == null)
                    {
                        _result.AddEmbossingRecord(_embossingData[i].AccountNumber, "not matched with any record in Excel file !");
                    }
                }
            }


            else if (_embossingData.Count > _excelData.Count)
            {
                for (int j = 0; j < _embossingData.Count; j++)
                {
                    var _ExcelRecord = _excelData.FirstOrDefault(e => e.AccountNumber == _embossingData[j].AccountNumber);
                    if (_ExcelRecord != null)
                    {

                        _result.CommonData.Add(new FinalResultDto
                        {
                            PAN = _embossingData[j].PAN,
                            EXP = _embossingData[j].EXP,
                            NAME = _embossingData[j].NAME.Trim(),
                            CVV2 = _embossingData[j].CVV2,
                            Track1 = _embossingData[j].Track1,
                            Track2 = _embossingData[j].Track2,
                            FarmerNumber = _ExcelRecord.FarmerNumber,
                            AccountNumber = _embossingData[j].AccountNumber,
                            chip = _embossingData[j].chip
                        });

                    }
                    else
                    {
                        _result.AddEmbossingRecord(_excelData[j].AccountNumber, "not matched with any record in excel file ! ");
                    }
                }

                for (int i = 0; i < _excelData.Count; i++)
                {
                    var _excelrecord = _result.CommonData.FirstOrDefault(e => e.AccountNumber == _excelData[i].AccountNumber);
                    if (_excelrecord == null)
                    {
                        _result.AddExcelRecord(_embossingData[i].AccountNumber, "not matched with any record in embossing file !");
                       
                    }
                }
            }

            else
            {
                for (int j = 0; j < _excelData.Count; j++)
                {
                    var _embossingRecord = _embossingData.FirstOrDefault(e => e.AccountNumber == _excelData[j].AccountNumber);
                    if (_embossingRecord != null)
                    {

                        _result.CommonData.Add(new FinalResultDto
                        {
                            PAN = _embossingRecord.PAN,
                            EXP = _embossingRecord.EXP,
                            NAME = _embossingRecord.NAME.Trim(),
                            CVV2 = _embossingRecord.CVV2,
                            Track1 = _embossingRecord.Track1,
                            Track2 = _embossingRecord.Track2,
                            FarmerNumber = _excelData[j].FarmerNumber,
                            AccountNumber = _embossingRecord.AccountNumber,
                            chip = _embossingRecord.chip
                        });

                    }
                    else
                    {
                        _result.AddExcelRecord(_excelData[j].AccountNumber, "not matched with any record in embossing file ! ");
                    }
                }

                for (int i = 0; i < _embossingData.Count; i++)
                {
                    var _embossingRecord = _result.CommonData.FirstOrDefault(e => e.AccountNumber == _embossingData[i].AccountNumber);
                    if (_embossingRecord == null)
                    {
                        _result.AddEmbossingRecord(_embossingData[i].AccountNumber, "not matched with any record in Excel file !");
                    }
                }
            }


        }
    }
}


