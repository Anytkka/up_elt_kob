using MySql.Data.MySqlClient;
using System;

namespace Project.Classes.Common
{
    public class Connection
    {
        public static readonly string config = "server=127.0.0.1; port = 3306; uid=root;pwd=;database=project;";

        public static MySqlConnection OpenConnection()
        {
            MySqlConnection connection = new MySqlConnection(config);
            connection.Open();

            return connection;
        }

        public static MySqlDataReader Query(string SQL, MySqlConnection connection)
        {
            return new MySqlCommand(SQL, connection).ExecuteReader();
        }

        public static void CloseConnection(MySqlConnection connection)
        {
            
            connection.Close();
            MySqlConnection.ClearPool(connection);
          
        }
    }
}
