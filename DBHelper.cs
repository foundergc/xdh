using System.Data;
using System.Data.SqlClient;

namespace PreCheckoutActions
{
    class DBHelper
    {
        private static readonly string connectionString = "server = 172.21.32.12;database = KBLIVE;uid = kbinplan;pwd = kb@inplan; Connect Timeout=30";

        public static DataTable GetTable(string sql)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, connection);
                dataAdapter.SelectCommand.CommandTimeout = 0;
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                return dataSet.Tables[0];
            }
        }

        public static SqlDataReader GetDataReader(string sql)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand(sql, connection);
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}
