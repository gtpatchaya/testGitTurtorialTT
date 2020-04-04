using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace GT.Tamroi.Api.Notic
{
    public class SqlServerDatabase
    {
        public SqlConnection GetSqlConnection(string server, string database, string user, string password)
        {
            string conString = "server = " + server + ";database = " + database + ";User ID = " + user + ";password = " + password + "; Connection Timeout = 0;";
            SqlConnection con = new SqlConnection(conString);
            return con;
        }
        public DataTable GetDataTableFromStore(SqlConnection sqlCon, string storeName, SqlParameter[] sqlParameter)
        {
            DataTable dt = new DataTable();
            try
            {
                string q2 = storeName;

                SqlCommand cmd = sqlCon.CreateCommand();

                //if (input.timeout > 0)
                //    cmd.CommandTimeout = input.timeout;

                cmd.CommandText = q2;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(sqlParameter);

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                da.Fill(dt);

                cmd.Dispose();
                return dt;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public DataTable SelectData(SqlConnection sqlCon, string queryComand, SqlParameter[] sqlParameter)
        {
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = queryComand;
            sqlCommand.Connection = sqlCon;
            if (sqlParameter != null)
            {
                sqlCommand.Parameters.AddRange(sqlParameter);
            }
            SqlDataAdapter a = new SqlDataAdapter(sqlCommand);
            a.SelectCommand.CommandTimeout = 0;

            DataTable t = new DataTable();
            a.Fill(t);
            a.Dispose();
            return t;
        }


    }
}