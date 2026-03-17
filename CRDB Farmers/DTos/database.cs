using Microsoft.Office.Interop.Access;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.Access.Application;
namespace CRDB_Farmers.DTos
{
    internal class database
    {
        private string TableFieldsCreate { get; set; } = string.Empty;
        private DataBaseConfigModel _DBconfig;
        private readonly OleDbConnectionStringBuilder _builder =
            new OleDbConnectionStringBuilder { Provider = "microsoft.ace.oledb.12.0" };


        public database(DataBaseConfigModel DBconfig)
        {
            _DBconfig = DBconfig;
            _builder.DataSource = DBconfig.DataBaseName;
        }




        /// <summary>
        /// Create database, one table and insert one record
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {

            if (File.Exists(_DBconfig.DataBaseName))
            {
                File.Delete(_DBconfig.DataBaseName);
            }

            if (Create())
            {
                make_create_query();
                return CreateTable();
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// Create database
        /// </summary>
        /// <returns></returns>
        private bool Create()
        {

            try
            {
                Application app;
                app = new Application();

                app.NewCurrentDatabase(
                    _DBconfig.DataBaseName,
                    AcNewDatabaseFormat.acNewDatabaseFormatAccess2000,
                    Type.Missing);

                app.CloseCurrentDatabase();
                Marshal.FinalReleaseComObject(app);
                //app = null;

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "Error");

                return false;
            }

        }


        //*****************************************
        //private void make_create_query()
        //{
        //    string[] fields = _DBconfig.Table_fields.Split(',');
        //    for (int i = 0; i < fields.Length; i++)
        //    {
        //        TableFieldsCreate = TableFieldsCreate + "[" + fields[i].Trim() + "] TEXT(255)";
        //        if (i == _DBconfig.UNIQUE) { TableFieldsCreate = TableFieldsCreate + " UNIQUE"; }
        //        if (i < fields.Length - 1) { TableFieldsCreate = TableFieldsCreate + ","; }
        //    }
        //}
        private void make_create_query()
        {
            string[] fields = _DBconfig.Table_fields.Split(',');
            for (int i = 0; i < fields.Length; i++)
            {
                string fieldName = fields[i].Trim();
                string dataType = "TEXT(255)"; 

                if (_DBconfig.FieldTypes.TryGetValue(fieldName, out string type))
                {
                    dataType = type;
                }

                TableFieldsCreate += $"[{fieldName}] {dataType}";
                if (i == _DBconfig.UNIQUE) TableFieldsCreate += " UNIQUE";
                if (i < fields.Length - 1) TableFieldsCreate += ",";
            }
        }


        /// <summary>
        /// Create table, insert one record
        /// </summary>
        /// <returns></returns>
        private bool CreateTable()
        {
            using (var cn = new OleDbConnection(_builder.ToString()))
            {

                using (var cmd = new OleDbCommand("", cn))
                {
                    _DBconfig.TableName = "Cards";
                    cmd.CommandText = "CREATE TABLE " + _DBconfig.TableName + " ([IDWAutoNumber] COUNTER PRIMARY KEY," + TableFieldsCreate + ")";

                    try
                    {
                        cn.Open();
                        // create table

                        cmd.ExecuteNonQuery();
                        cn.Close();
                        cn.Dispose();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.ToString(), "Error");
                        return false;
                    }

                }
            }
        }

      




    }
}
