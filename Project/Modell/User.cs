using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Windows;

namespace Project.Classes
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Biography { get; set; }
        public string ProfileImagePath { get; set; }

        public User(int Id, string Email, string Password, string FullName, string Biography, string ProfileImagePath = null)
        {
            this.Id = Id;
            this.Email = Email;
            this.Password = Password;
            this.FullName = FullName;
            this.Biography = Biography;
            this.ProfileImagePath = ProfileImagePath;
        }

        public static User GetUserByCredentials(string email, string password)
        {
            User user = null;
            var connection = Connection.OpenConnection();

            try
            {
                string query = "SELECT * FROM users WHERE email = @email AND password = @password";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@password", password);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User(
                            reader.GetInt32("id"),
                            reader.GetString("email"),
                            reader.GetString("password"),
                            reader.GetString("full_name"),
                            reader.IsDBNull(reader.GetOrdinal("biography")) ? null : reader.GetString("biography"),
                            reader.IsDBNull(reader.GetOrdinal("image")) ? null : reader.GetString("image")
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}");
            }
            finally
            {
                Connection.CloseConnection(connection);
            }

            return user;
        }
    }
}