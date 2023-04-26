using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAY_Weighing
{

    static class User
    {
        private static string conn = DbManager.GetConnectString();
        public static string UserPassword { get; set; }

        public static string MANUFACTURER_PASSWORD = "1992";


        public static System.Data.DataTable LoadUserInfo()
        {
            try
            {
                System.Data.DataTable table = DbManager.LoadDataTable(DbSchema.User.NAME);
                UserPassword = table.Rows[0][1].ToString();
                return table;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Common.Logger.LogInfo(ex.ToString());
                return null;
            }
        }

        private static void DeleteLastRow()
        {
            System.Data.DataTable table = null;
            SqlHelper sqlHelper = new SqlHelper();
            string query = $"DELETE FROM LivaUserInfo WHERE ID = (SELECT Max(ID) FROM LivaUserInfo)";
            sqlHelper.SelectCmdByQuery(conn, query, out table);
            string query2 = $"declare @max int select @max=max([Id]) from [LivaUserInfo] if @max IS NULL SET @max = 0 DBCC CHECKIDENT ('LivaUserInfo', RESEED, @max)";
            sqlHelper.SelectCmdByQuery(conn, query2, out table);
        }

        public static bool ChangePassword(string oldPassword, string newPassword, string newPasswordAgain)
        {
            try
            {
                if (((oldPassword == UserPassword) && (newPasswordAgain == newPassword))
                    || oldPassword == MANUFACTURER_PASSWORD)
                {
                    SqlHelper sqlHelper = new SqlHelper();

                    string query = "INSERT INTO LivaUserInfo (Password) Values('" + newPassword.ToString() + "')";
                    DeleteLastRow();
                    sqlHelper.ExecuteQuery(conn, query);
                    LoadUserInfo();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
