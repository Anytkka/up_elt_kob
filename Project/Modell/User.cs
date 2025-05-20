using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project.Classes
{
    public class User
    {
        //Поля почта, ФИО, пароль обязательны для заполнения.
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Biography { get; set; }
        public string ProfileImagePath { get; internal set; }

        public User(int Id, string Email, string Password, string FullName, string Biography)
        {
            this.Id = Id;
            this.Email = Email;
            this.Password = Password;
            this.FullName = FullName;
            this.Biography = Biography;
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
                            reader.IsDBNull(reader.GetOrdinal("biography")) ? null : reader.GetString("biography")
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