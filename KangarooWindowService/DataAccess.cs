using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Security.Cryptography;
using System.IO;

namespace KangarooWindowService
{
    public class DataAccess
    {
        SqlConnection conn;
        SqlCommand command;
        SqlDataAdapter adp;
        Log_Sytems logs;
        Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public DataAccess(string connStr)
        {
            try
            {
                conn = new SqlConnection(connStr);
                command = conn.CreateCommand();
            }
            catch (SqlException ex)
            {
                logs = new Log_Sytems();
                logs.ErrorLog(ex.Message, ex.StackTrace);
            }
        }

        public DataAccess()
        {
            try
            {
                string strConnect = DataAccess.Decrypt(ConfigurationManager.AppSettings["ConnectERP"].ToString());
               

                conn = new SqlConnection(strConnect);
                command = conn.CreateCommand();
            }
            catch (SqlException ex)
            {
                logs = new Log_Sytems();
                logs.ErrorLog(ex.Message, ex.StackTrace);
            }
        }

        public static string Decrypt(string stringToDecrypt)
        {
            return stringToDecrypt;
        }

        public SqlConnection getConnection()
        {
            return conn;
        }


        public bool CheckConnection()
        {
            bool kt = false;

            try
            {
                conn.Open();
                if (conn.State == ConnectionState.Closed)
                {
                    kt = false;
                }
                else
                {

                    kt = true;
                }
            }
            catch (Exception ex)
            {
                kt = false;
            }

            return kt;

        }



        public DataTable ExecuteQueryDataSet(string strSQL, CommandType ct, List<SqlParameter> paramList)
        {
            conn.Close();
            conn.Open();
            // command = new OracleCommand();
            DataTable ds = null;
            //command.Connection = conn;
            command.Parameters.Clear();
            command.CommandText = strSQL;
            string paramName = "";
            command.CommandType = ct;
            try
            {
                if (paramList != null)
                    foreach (SqlParameter p in paramList)
                    {
                        command.Parameters.Add(p);
                        paramName += p.ParameterName + ":" + p.Value + "|";
                    }
                adp = new SqlDataAdapter(command);
                ds = new DataTable();
                adp.Fill(ds);
            }
            catch (SqlException ex)
            {

                logs = new Log_Sytems();
                logs.ErrorLog(ex.Data + ex.Message + ex.ErrorCode + ex.Source + ex.InnerException + "\r\nQuery:" + strSQL + "\r\nParam:" + paramName, ex.StackTrace);
            }
            finally
            {
                conn.Close();
            }
            return ds;
        }
        public bool MyExecuteNonQuery(string strSQL, CommandType ct, ref string error, List<SqlParameter> paramList)
        {
            bool f = false;
            string paramName = "";
            try
            {
                conn.Close();
                conn.Open();
                command.Parameters.Clear();
                command.CommandText = strSQL;
                command.CommandType = ct;
                if (paramList != null)
                    foreach (SqlParameter p in paramList)
                    {

                        paramName += p.ParameterName + ":" + p.Value + "|";
                        command.Parameters.Add(p);
                    }
                command.ExecuteNonQuery();
                f = true;
            }
            catch (SqlException ex)
            {
                error = ex.Message;
                logs = new Log_Sytems();
                logs.ErrorLog(ex.Data + ex.Message + ex.ErrorCode + ex.Source + ex.InnerException + "\r\nQuery:" + strSQL + "\r\nParam:" + paramName, ex.StackTrace);
            }
            finally
            {
                conn.Close();
            }
            return f;
        }


        public object MyExecuteScalar(string strSQL, CommandType ct, ref string error, List<SqlParameter> paramList)
        {
            object f = null;
            string paramName = "";
            try
            {
                conn.Close();
                conn.Open();
                command.Parameters.Clear();
                command.CommandText = strSQL;
                command.CommandType = ct;
                if (paramList != null)
                    foreach (SqlParameter p in paramList)
                        command.Parameters.Add(p);
                f = command.ExecuteScalar();
            }
            catch (SqlException ex)
            {
                error = ex.Message;
                logs = new Log_Sytems();
                logs.ErrorLog(ex.Data + ex.Message + ex.ErrorCode + ex.Source + ex.InnerException + "\r\nQuery:" + strSQL + "\r\nParam:" + paramName, ex.StackTrace);
            }
            finally
            {
                conn.Close();
            }
            return f;
        }


    }
}
