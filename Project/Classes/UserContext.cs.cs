using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;

namespace Project.Classes
{
    public class UserContext : User
    {
        public UserContext(int Id, string Email, string Password, string FullName, string Biography, string ProfileImagePath = null)
            : base(Id, Email, Password, FullName, Biography, ProfileImagePath) { }

        public static List<UserContext> Get()
        {
            List<UserContext> AllUser = new List<UserContext>();
            string SQL = "SELECT * FROM `user`;";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader Data = Connection.Query(SQL, connection);
            while (Data.Read())
            {
                AllUser.Add(new UserContext(
                    Data.GetInt32(0),
                    Data.GetString(1),
                    Data.GetString(2),
                    Data.GetString(3),
                    Data.IsDBNull(4) ? null : Data.GetString(4),
                    Data.IsDBNull(5) ? null : Data.GetString(5)
                ));
            }
            Connection.CloseConnection(connection);
            return AllUser;
        }

        public void Add()
        {
            string SQL = "INSERT INTO `user` (`email`, `password`, `fullName`, `biography`, `image`) " +
                         "VALUES (@email, @password, @fullName, @biography, @image)";

            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                cmd.Parameters.AddWithValue("@email", this.Email);
                cmd.Parameters.AddWithValue("@password", this.Password);
                cmd.Parameters.AddWithValue("@fullName", this.FullName);
                cmd.Parameters.AddWithValue("@biography", this.Biography ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@image", this.ProfileImagePath ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            Connection.CloseConnection(connection);
        }

        public void Update()
        {
            string SQL = "UPDATE `user` " +
                 "SET `email`=@email, " +
                 "`password`=@password, " +
                 "`fullName`=@fullName, " +
                 "`biography`=@biography, " +
                 "`image`=@image " +
                 "WHERE `id`=@id";

            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                cmd.Parameters.AddWithValue("@id", this.Id);
                cmd.Parameters.AddWithValue("@email", this.Email);
                cmd.Parameters.AddWithValue("@password", this.Password);
                cmd.Parameters.AddWithValue("@fullName", this.FullName);
                cmd.Parameters.AddWithValue("@biography", this.Biography ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@image", this.ProfileImagePath ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            Connection.CloseConnection(connection);
        }

        public void Delete()
        {
            string SQL = "DELETE FROM `user` WHERE `id`=@id";
            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                cmd.Parameters.AddWithValue("@id", this.Id);
                cmd.ExecuteNonQuery();
            }
            Connection.CloseConnection(connection);
        }
    }
}