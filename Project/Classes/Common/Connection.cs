using MySql.Data.MySqlClient;
using System;

namespace Project.Classes.Common
{
    public class Connection
    {
        // Ensure the connection string is correct
        public static readonly string config = "server=127.0.0.1;port=3307;uid=root;pwd=;database=project;";

        public static MySqlConnection OpenConnection()
        {
            MySqlConnection connection = new MySqlConnection(config);
            try
            {
                connection.Open();
                return connection;
            }
            catch (MySqlException ex)
            {
                // Log the exception details for debugging
                Console.WriteLine($"Error connecting to MySQL: {ex.Message}");
                throw; // Re-throw the exception to handle it in the calling method
            }
        }

        public static MySqlDataReader Query(string SQL, MySqlConnection connection)
        {
            return new MySqlCommand(SQL, connection).ExecuteReader();
        }

        public static void CloseConnection(MySqlConnection connection)
        {
            if (connection != null)
            {
                connection.Close();
                MySqlConnection.ClearPool(connection);
            }
        }
    }
}
