using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace CAY_Weighing
{
    public interface IDbHelper
    {
        bool Open(string connectionString, bool showErrorMessage);
        void Close();
        bool IsOpen();
        bool SelectCmdByQuery(string query, out DataTable dataTable, bool showErrorMessage);
        bool SelectCmdByQuery(string connectionString, string query, out DataTable dataTable, bool showErrorMessage);
        bool ExecuteCmd(string tableName, DataTable dataTable, bool showErrorMessage);
        bool ExecuteCmd(string connectionString, string tableName, DataTable dataTable, bool showErrorMessage);
        bool ExecuteQuery(string query, bool showErrorMessage);
        bool ExecuteQuery(string connectionString, string query, bool showErrorMessage);
        DbTransaction BeginTransaction();
        void Commit();
        void Rollback();

    }
}
