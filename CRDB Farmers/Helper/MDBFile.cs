using CRDB_Farmers.DTos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDB_Farmers.Helper
{
    internal static class MDBFile
    {

        private static DataTable ConvertListToDataTable<T>(List<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                    values[i] = props[i].GetValue(item, null);

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static bool CreateMDBFile(List<FinalResultDto> records, string path, string tableName, ref string errorMessage)
        {
            try
            {
                path = Path.Combine(path, $"{tableName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.mdb");

                // Convert list of T to DataTable
                DataTable dataTable = ConvertListToDataTable(records);

                DataBaseConfigModel config = new DataBaseConfigModel();
                config.Table_fields = string.Join(",", dataTable.Columns.Cast<DataColumn>()
                    .Select(c => "IDW" + c.ColumnName));
                config.FieldTypes = dataTable.Columns.Cast<DataColumn>()
                    .ToDictionary(c => "IDW" + c.ColumnName, c => GetAccessDataType(c.DataType));


                config.DataBaseName = path;
                config.TableName = tableName;
                config.UNIQUE = -1;

                database db = new database(config);
                if (!db.Execute())
                {
                    errorMessage = "Couldn't create database file Error CreateMDBFile";
                    return false;
                }
                InsertDataIntoAccess(records, path,tableName,ref errorMessage);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"{ex.ToString()} Error CreateMDBFile";
                return false;
            }
        }


        private static string GetAccessDataType(Type type)
        {
            if (type == typeof(string))
                return "TEXT";
            if (type == typeof(int) || type == typeof(int?))
                return "INTEGER";
            if (type == typeof(short) || type == typeof(short?))
                return "SMALLINT";
            if (type == typeof(long) || type == typeof(long?))
                return "BIGINT";
            if (type == typeof(bool) || type == typeof(bool?))
                return "YESNO";
            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return "DATETIME";
            if (type == typeof(decimal) || type == typeof(float) || type == typeof(double))
                return "DOUBLE";
            if (type == typeof(byte[]))
                return "OLEOBJECT"; 
            return "TEXT"; 
        }

        public static bool InsertDataIntoAccess(List<FinalResultDto> records, string mdbFilePath, string tableName, ref string errorMessage)
        {
            try
            {
                string connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={mdbFilePath};";

                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    foreach (var record in records)
                    {
                        string query = $@"
                    INSERT INTO Cards (
                        IDWPAN, IDWEXP, IDWNAME, IDWCVV2, IDWTrack1, IDWTrack2, IDWchip, IDWFarmerNumber, IDWAccountNumber
                    ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

                        using (OleDbCommand cmd = new OleDbCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@IDWPAN", record.PAN);
                            cmd.Parameters.AddWithValue("@IDWEXP", record.EXP);
                            cmd.Parameters.AddWithValue("@IDWNAME", record.NAME);
                            cmd.Parameters.AddWithValue("@IDWCVV2", record.CVV2);
                            cmd.Parameters.AddWithValue("@IDWTrack1", record.Track1);
                            cmd.Parameters.AddWithValue("@IDWTrack2", record.Track2);
                            //cmd.Parameters.Add("@chip", record.chip);
                            var chipParam = cmd.Parameters.Add("IDWchip", OleDbType.LongVarBinary);
                            chipParam.Value = record.chip ?? new byte[0];
                            cmd.Parameters.AddWithValue("@IDWFarmerNumber", record.FarmerNumber);
                            cmd.Parameters.AddWithValue("@IDWAccountNumber", record.AccountNumber);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    conn.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Insert failed: {ex.Message}";
                return false;
            }
        }


    }
}
