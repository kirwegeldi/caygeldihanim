using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;

namespace CAY_Weighing
{
    class DbManager
    {
        private static readonly SqlHelper _dbHelper = new SqlHelper();

        public static string GetConnectString()
        {
            //string conn = $"User ID={AppInfo.ErpDb.User};Password={AppInfo.ErpDb.Password};" +
            //              $"Initial Catalog={AppInfo.ErpDb.Database};Data Source={AppInfo.ErpDb.Server}";

            //string conn = $"User ID=sa;Password=12345;" +
            //              $"Initial Catalog=MainStation;Data Source=10.45.70.56";

            //string conn = "DESKTOP-8HGS2M7\\SQLEXPRESS;Initial Catalog=CAS;Integrated Security=True";
            ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;
            string conn = settings["databaseConnection"].ToString();
            return conn;
        }

        public static bool SaveDataTable(string tableName, System.Data.DataTable dataTable)
        {
            bool result = false;

            if (_dbHelper.Open(GetConnectString(), false))
            {
                _dbHelper.BeginTransaction();

                result = _dbHelper.ExecuteCmd(tableName, dataTable);

                if (result)
                {
                    _dbHelper.Commit();
                }
                else
                {
                    _dbHelper.Rollback();
                }

                _dbHelper.Close();
            }

            return result;
        }

        public static System.Data.DataTable LoadDataTable(string tableName)
        {
            string sql = $"SELECT * FROM {tableName}";

            _dbHelper.SelectCmdByQuery(GetConnectString(), sql, out System.Data.DataTable table);
          
            return table;
        }
    }
}
