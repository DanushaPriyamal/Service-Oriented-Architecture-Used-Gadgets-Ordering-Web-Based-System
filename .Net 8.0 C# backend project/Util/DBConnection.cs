using Microsoft.Data.SqlClient;

namespace TheGadgetHub.Util
{
    public class DBConnection
    {
        private SqlConnection connection;
        public DBConnection()
        {
            var Constring = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["SNCon"];
            connection = new SqlConnection(Constring);
        }

        public SqlConnection GetConn()
        {
            return connection;
        }

        public void ConOpen()
        {
            connection.Open();
        }

        public void ConClose()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }
    }
}
