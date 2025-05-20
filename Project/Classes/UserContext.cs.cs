using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System.Collections.Generic;

namespace Project.Classes
{
    public class UserContext : User
    {
        public UserContext(int Id, string Email, string Password, string FullName, string Biography) : base(Id,  Email, Password, FullName, Biography) {}

        public static List<UserContext> Get()
        {   
            List<UserContext> AllUser = new List<UserContext>();
            string SQL = $"SELECT * FROM `user`;";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader Data = Connection.Query(SQL, connection);
            while (Data.Read())
            {
                AllUser.Add(new UserContext(
                    Data.GetInt32(0),
                    Data.GetString(1),
                    Data.GetString(2),
                    Data.GetString(3),
                    Data.GetString(4)
                ));
            }
            Connection.CloseConnection(connection);
            return AllUser;
        }
        public void Add() 
        {
            string SQL = "INSERT INTO " +
                            "`user` ( " +
                            "`email`, " +
                            "`password`, " +
                            "`fullName`, " +
                            "`biography`) " +
                        "VALUES (" +
                            $"'{this.Email}', " +
                            $"'{this.Password}', " +
                            $"'{this.FullName}', " +
                            $"'{this.Biography}')";

            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        public void Update()
        {
            string SQL = "UPDATE " +
                            "`user` " +
                        "SET " +
                                $"`email`='{this.Email}', " +
                                $"`password`='{this.Password}', " +
                                $"`fullName`='{this.FullName}', " +
                                $"`biography`='{this.Biography}' " +
                        "WHERE " +
                                $"`id`='{this.Id}'";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        public void Delete() 
        {
            string SQL = $"DELETE FROM `user` WHERE `id`='{this.Id}'";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }
    }
}