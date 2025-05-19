using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;

namespace Project.Classes
{
    public class ProjUserContext : ProjUser
    {
        public ProjUserContext(int id, int project, int user, string role)
            : base(id, project, user, role) { }

        // Получает все связи пользователей 
        public static List<ProjUserContext> Get()
        {
            List<ProjUserContext> allProjUsers = new List<ProjUserContext>();
            string SQL = "SELECT * FROM `ProjUsers`;";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader data = Connection.Query(SQL, connection);

            while (data.Read())
            {
                allProjUsers.Add(new ProjUserContext(
                    data.GetInt32(0),    
                    data.GetInt32(1),    
                    data.GetInt32(2),     
                    data.GetString(3)     
                ));
            }

            Connection.CloseConnection(connection);
            return allProjUsers;
        }

        // Получает связь по ID
        public static ProjUserContext GetById(int id)
        {
            string SQL = $"SELECT * FROM `ProjUsers` WHERE `id`='{id}'";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader data = Connection.Query(SQL, connection);

            if (data.Read())
            {
                var projUser = new ProjUserContext(
                    data.GetInt32(0),
                    data.GetInt32(1),
                    data.GetInt32(2),
                    data.GetString(3)
                );
                Connection.CloseConnection(connection);
                return projUser;
            }

            Connection.CloseConnection(connection);
            return null;
        }

        // Получает все связи для конкретного проекта
        public static List<ProjUserContext> GetByProjectId(int projectId)
        {
            List<ProjUserContext> projUsers = new List<ProjUserContext>();
            string SQL = $"SELECT * FROM `ProjUsers` WHERE `project`='{projectId}'";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader data = Connection.Query(SQL, connection);

            while (data.Read())
            {
                projUsers.Add(new ProjUserContext(
                    data.GetInt32(0),
                    data.GetInt32(1),
                    data.GetInt32(2),
                    data.GetString(3)
                ));
            }

            Connection.CloseConnection(connection);
            return projUsers;
        }

        // Получает все связи для конкретного пользователя
        public static List<ProjUserContext> GetByUserId(int userId)
        {
            List<ProjUserContext> projUsers = new List<ProjUserContext>();
            string SQL = $"SELECT * FROM `ProjUsers` WHERE `user`='{userId}'";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader data = Connection.Query(SQL, connection);

            while (data.Read())
            {
                projUsers.Add(new ProjUserContext(
                    data.GetInt32(0),
                    data.GetInt32(1),
                    data.GetInt32(2),
                    data.GetString(3)
                ));
            }

            Connection.CloseConnection(connection);
            return projUsers;
        }

        // Добавляет новую связь
        public void Add()
        {
            string SQL = $"INSERT INTO `ProjUsers` (`project`, `user`, `role`) " +
                         $"VALUES ({this.Project}, {this.User}, '{this.Role}')";

            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        // Обновляет существующую связь
        public void Update()
        {
            string SQL = $"UPDATE `ProjUsers` SET " +
                        $"`project`={this.Project}, " +
                        $"`user`={this.User}, " +
                        $"`role`='{this.Role}' " +
                        $"WHERE `id`={this.Id}";

            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        // Удаляет связь из БД
        public void Delete()
        {
            string SQL = $"DELETE FROM `ProjUsers` WHERE `id`={this.Id}";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }
    }
}