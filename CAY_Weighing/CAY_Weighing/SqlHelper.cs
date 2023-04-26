using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Sql;
using System.Data;
using System.IO;
using System.Data.Common;
using System.Data.SqlClient;

namespace CAY_Weighing
{
    public class SqlHelper : IDbHelper
    {
        private SqlConnection _dbConnection;
        private SqlTransaction _dbTransaction;

        public SqlHelper()
        {
            _dbConnection = new SqlConnection();
        }

        public bool Open(string connectionString, bool showErrorMessage = true)
        {
            bool opened = false;

            try
            {
                _dbConnection.ConnectionString = connectionString;
                _dbConnection.Open();

                opened = true;
            }
            catch (Exception ex)
            {
                Common.Logger.LogError(ex.ToString());
                opened = false;
            }
            return opened;
        }

        public void Close()
        {
            if (_dbConnection == null)
                return;

            _dbConnection.Close();
        }

        public bool IsOpen()
        {
            return _dbConnection.State == ConnectionState.Open;
        }

        public bool SelectCmdByQuery(string query, out DataTable dataTable, bool showErrorMessage = true)
        {
            bool result = false;
            dataTable = new DataTable();

            try
            {
                SqlCommand dbCommand = new SqlCommand(query, _dbConnection);
                dbCommand.Transaction = _dbTransaction;
                SqlDataReader dbDataReader = dbCommand.ExecuteReader();

                dataTable.Load(dbDataReader);

                result = true;
            }
            catch (Exception ex)
            {
                Common.Logger.LogError(ex.ToString());
            }

            return result;
        }

        public bool SelectCmdByQuery(string connectionString, string query, out DataTable dataTable, bool showErrorMessage = true)
        {
            bool result = false;
            dataTable = new DataTable();

            try
            {
                using (SqlConnection dbConnection = new SqlConnection(connectionString))
                {
                    dbConnection.Open();

                    SqlCommand dbCommand = new SqlCommand(query, dbConnection);
                    SqlDataReader dbDataReader = dbCommand.ExecuteReader();

                    dataTable.Load(dbDataReader);
                }

                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Common.Logger.LogError(ex.ToString());
            }

            return result;
        }

        public bool ExecuteCmd(string tableName, DataTable dataTable, bool showErrorMessage = true)
        {
            bool result = false;
            string selectQuery = string.Format("SELECT * FROM {0}", tableName);

            if (Convert.IsDBNull(dataTable))
                return result;

            try
            {
                SqlDataAdapter dbDataAdapter = new SqlDataAdapter();
                dbDataAdapter.SelectCommand = new SqlCommand(selectQuery, _dbConnection);
                dbDataAdapter.SelectCommand.Transaction = _dbTransaction;

                SqlCommandBuilder dbCommandBuilder = new SqlCommandBuilder(dbDataAdapter);

                if (_dbTransaction != null)
                {
                    dbDataAdapter.InsertCommand = dbCommandBuilder.GetInsertCommand();
                    dbDataAdapter.UpdateCommand = dbCommandBuilder.GetUpdateCommand();
                    dbDataAdapter.DeleteCommand = dbCommandBuilder.GetDeleteCommand();

                    dbDataAdapter.InsertCommand.Transaction = _dbTransaction;
                    dbDataAdapter.UpdateCommand.Transaction = _dbTransaction;
                    dbDataAdapter.DeleteCommand.Transaction = _dbTransaction;
                }

                dbDataAdapter.Update(dataTable);

                result = true;
            }
            catch (Exception ex)
            {
                Common.Logger.LogError(ex.ToString());
            }

            return result;
        }

        public bool ExecuteCmd(string connectionString, string tableName, DataTable dataTable, bool showErrorMessage)
        {
            bool result = false;
            string selectQuery = string.Format("SELECT * FROM {0}", tableName);

            if (Convert.IsDBNull(dataTable))
                return result;

            try
            {
                using (SqlConnection dbConnection = new SqlConnection(connectionString))
                {
                    dbConnection.Open();

                    SqlDataAdapter dbDataAdapter = new SqlDataAdapter();
                    dbDataAdapter.SelectCommand = new SqlCommand(selectQuery, dbConnection);
                    SqlCommandBuilder dbCommandBuilder = new SqlCommandBuilder(dbDataAdapter);

                    dbDataAdapter.Update(dataTable);
                }

                result = true;
            }
            catch (Exception ex)
            {
                Common.Logger.LogError(ex.ToString());
            }

            return result;
        }

        public bool ExecuteQuery(string query, bool showErrorMessage = true)
        {
            bool result = false;

            if (string.IsNullOrEmpty(query))
                return result;

            try
            {
                SqlCommand dbCommand = new SqlCommand(query, _dbConnection);
                dbCommand.Transaction = _dbTransaction;
                dbCommand.ExecuteNonQuery();

                result = true;
            }
            catch (Exception ex)
            {
                Common.Logger.LogError(ex.ToString());
            }

            return result;
        }

        public bool ExecuteQuery(string connectionString, string query, bool showErrorMessage = true)
        {
            bool result = false;

            if (string.IsNullOrEmpty(query))
                return result;

            try
            {
                using (SqlConnection dbConnection = new SqlConnection(connectionString))
                {
                    dbConnection.Open();

                    SqlCommand dbCommand = new SqlCommand(query, dbConnection);
                    dbCommand.ExecuteNonQuery();
                }

                result = true;
            }
            catch (Exception ex)
            {
                Common.Logger.LogError(ex.ToString());
            }

            return result;
        }

        public DbTransaction BeginTransaction()
        {
            if (_dbConnection != null)
            {
                _dbTransaction = _dbConnection.BeginTransaction();
            }

            return _dbTransaction;
        }

        public void Commit()
        {
            if (_dbTransaction != null)
            {
                _dbTransaction.Commit();
                _dbTransaction = null;
            }
        }

        public void Rollback()
        {
            if (_dbTransaction != null)
            {
                _dbTransaction.Rollback();
                _dbTransaction = null;
            }
        }
    }
}
