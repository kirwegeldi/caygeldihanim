using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CAY_Weighing.Properties;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using System.Data;
using System.IO;

namespace CAY_Weighing
{
    internal static class Report
    {
        private static string conn = DbManager.GetConnectString();
        public static double Silo1 { get; set; }
        public static double Silo2 { get; set; }
        public static double Silo3 { get; set; }
        public static double Silo4 { get; set; }
        public static double Silo5 { get; set; }
        public static double Silo6 { get; set; }
        public static double Silo7 { get; set; }
        public static double Silo8 { get; set; }
        public static double Hat1 { get; set; }
        public static double Hat2 { get; set; }

        private static double[] filledSilo;


        public static void SaveFillingData(Dictionary<int, double> fillingInfo)
        {
            filledSilo = new double[9];
            System.Data.DataTable table = LoadDataDaily();

            if (table.Rows.Count == 0)                                  //Eğer bugün hiçbir data kaydedilmediyse şuanki dataları atayıp database'e gönderir.
            {
                for (int i = 0; i < filledSilo.Length; i++)
                {
                    filledSilo[i] = 0;
                }
                foreach(KeyValuePair<int, double> siloinfo in fillingInfo)
                {
                    double value;
                    fillingInfo.TryGetValue(siloinfo.Key, out value);
                    filledSilo[siloinfo.Key] = value;
                }
                Silo1 = filledSilo[1];
                Silo2 = filledSilo[2];
                Silo3 = filledSilo[3];
                Silo4 = filledSilo[4];
                Silo5 = filledSilo[5];
                Silo6 = filledSilo[6];
                Silo7 = filledSilo[7];
                Silo8 = filledSilo[8];

                double total = 0;

                for (int i = 1; i < 4; i++)
                {

                    total += filledSilo[i];
                }

                Hat1 = total;
                total = 0;

                for (int i = 4; i < 8; i++)
                {
                    total += filledSilo[i];
                }

                Hat2 = total;
            }
            else                                                            //Eğer sql'de bugune ait data varsa günceller ve ekler.
            {
                foreach (KeyValuePair<int, double> siloinfo in fillingInfo)
                {
                    double value;
                    fillingInfo.TryGetValue(siloinfo.Key, out value);
                    value += int.Parse(table.Rows[0][siloinfo.Key].ToString());
                    table.Rows[0][siloinfo.Key] = value.ToString();
                }
                for (int i =1; i < table.Columns.Count-3; i++)
                {
                    filledSilo[i] = int.Parse(table.Rows[0][i].ToString());
                }
                Silo1 = filledSilo[1];
                Silo2 = filledSilo[2];
                Silo3 = filledSilo[3];
                Silo4 = filledSilo[4];
                Silo5 = filledSilo[5];
                Silo6 = filledSilo[6];
                Silo7 = filledSilo[7];
                Silo8 = filledSilo[8];

                for (int i = 1; i < 5; i++)
                {
                    Hat1 += filledSilo[i];
                }
                for (int i = 5; i < 9; i++)
                {
                    Hat2 += filledSilo[i];
                }

                DeleteLastRow();
            }
            InsertRow();
        }

        public static bool ExportasExcel(System.Data.DataTable dataTable , string timeRange)
        {
            try
            {
                Excel.Application excel = new Excel.Application();
                Excel.Workbook workbook = excel.Workbooks.Add(Type.Missing);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.ActiveSheet;

                for (var i = 0; i < dataTable.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1] = dataTable.Columns[i].ColumnName;
                }



                // DataTable'dan verileri oku
                // rows
                for (var i = 0; i < dataTable.Rows.Count; i++)
                {
                    // to do: format datetime values before printing
                    for (var j = 0; j < dataTable.Columns.Count; j++)
                    {
                        worksheet.Cells[i + 2, j + 1] = dataTable.Rows[i][j];
                    }
                }
             
                // Excel dosyasını kaydet

                
                string fileName = "Result-";
                fileName += timeRange + ".xlsx";
                

                string path = AppDomain.CurrentDomain.BaseDirectory;

                path = Path.Combine(path, fileName);

                
                    workbook.SaveAs(path, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                      Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
                      Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            
               
                excel.Quit();
                return true;
            }
            catch { return false;}
        }
        public static System.Data.DataTable LoadAllData()
        {
            System.Data.DataTable table = DbManager.LoadDataTable(DbSchema.Result.NAME);

            return table;          
        }

        public static System.Data.DataTable LoadDataDaily()
        {

            System.Data.DataTable table = null;
            SqlHelper sqlHelper = new SqlHelper();

            string query = $"SELECT* FROM Result WHERE TARIH >= DATEADD(day," + "-1" + ", GETDATE()) ; ";
            sqlHelper.SelectCmdByQuery(DbManager.GetConnectString(), query, out table);
            return table;
        }

        public static System.Data.DataTable LoadMountly()
        {

            System.Data.DataTable table = null;
            SqlHelper sqlHelper = new SqlHelper();

            string query = $"SELECT* FROM Result WHERE TARIH >= DATEADD(day," + "-30" + ", GETDATE()) ; ";
            sqlHelper.SelectCmdByQuery(DbManager.GetConnectString(), query, out table);
            return table;
        }
        public static System.Data.DataTable Load3Mountly()
        {

            System.Data.DataTable table = null;
            SqlHelper sqlHelper = new SqlHelper();

            string query = $"SELECT* FROM Result WHERE TARIH >= DATEADD(day," + "-30" + ", GETDATE()) ; ";
            sqlHelper.SelectCmdByQuery(DbManager.GetConnectString(), query, out table);
            return table;
        }
        public static System.Data.DataTable Load6Mountly()
        {

            System.Data.DataTable table = null;
            SqlHelper sqlHelper = new SqlHelper();

            string query = $"SELECT* FROM Result WHERE TARIH >= DATEADD(day," + "-30" + ", GETDATE()) ; ";
            sqlHelper.SelectCmdByQuery(DbManager.GetConnectString(), query, out table);
            return table;
        }

        private static void InsertRow()
        {
            SqlHelper sqlHelper = new SqlHelper();

            string query = "INSERT INTO Result(SILO1,SILO2,SILO3,SILO4,SILO5,SILO6,SILO7,SILO8,HAT1,HAT2,TARIH) Values('"
                + Silo1.ToString() + "', '" + Silo2.ToString() + "', '" + Silo3.ToString() + "', '" + Silo4.ToString() + "', '" + Silo5.ToString() 
                + "', '" + Silo6.ToString() + "', '" + Silo7.ToString() + "', '" + Silo8.ToString() + "', '" 
                + Hat1.ToString() + "', '" + Hat2.ToString() + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
            Console.WriteLine(query);
            try
            {
                sqlHelper.ExecuteQuery(conn, query);
            }
            catch{}     
        }
        private static void DeleteLastRow()
        {
            System.Data.DataTable table = null;
            SqlHelper sqlHelper = new SqlHelper();
            string query = $"DELETE FROM Result  WHERE  ID = (SELECT Max(ID) FROM Result)";
            sqlHelper.SelectCmdByQuery(conn, query, out table);
            string query2 = $"declare @max int select @max=max([Id]) from [Result] if @max IS NULL SET @max = 0 DBCC CHECKIDENT ('[Result]', RESEED, @max)";
            sqlHelper.SelectCmdByQuery(conn, query2, out table);
        }
        public static bool  DeleteAllData(bool result)
        {
            if (result)
            {
                SqlHelper sqlHelper = new SqlHelper();

                string query = "TRUNCATE TABLE " + "Result";
                try
                {
                    sqlHelper.ExecuteQuery(conn, query);
                    return true;
                }
                catch { return false; }
            }
            else
            {
                return false;
            }
            
            
        }
    }
}
