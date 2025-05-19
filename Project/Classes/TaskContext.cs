using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;

namespace Project.Classes
{
    public class DocumentContext : Task
    {
        public DocumentContext(int id, string name, string description, DateTime dueDate, int status, int projectId)
            : base(id, name, description, dueDate, status, projectId) { }

        // Получает все задачи
        public static List<DocumentContext> Get()
        {
            List<DocumentContext> allTasks = new List<DocumentContext>();
            string SQL = "SELECT * FROM `Tasks`;";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader data = Connection.Query(SQL, connection);

            while (data.Read())
            {
                allTasks.Add(new DocumentContext(
                    data.GetInt32(0),   
                    data.GetString(1),   
                    data.GetString(2), 
                    data.GetDateTime(3), 
                    data.GetInt32(4),   
                    data.GetInt32(5)   
                ));
            }

            Connection.CloseConnection(connection);
            return allTasks;
        }

        // Получает задачу по ID 
        public static DocumentContext GetById(int id)
        {
            string SQL = $"SELECT * FROM `Tasks` WHERE `id`='{id}'";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader data = Connection.Query(SQL, connection);

            if (data.Read())
            {
                var task = new DocumentContext(
                    data.GetInt32(0),
                    data.GetString(1),
                    data.GetString(2),
                    data.GetDateTime(3),
                    data.GetInt32(4),
                    data.GetInt32(5)
                );
                Connection.CloseConnection(connection);
                return task;
            }

            Connection.CloseConnection(connection);
            return null;
        }

        // Добавляет новую задачу
        public void Add()
        {
            string SQL = $"INSERT INTO `Tasks` (`name`, `description`, `dueDate`, `status`, `projectId`) " +
                         $"VALUES ('{this.Name}', '{this.Description}', '{this.DueDate:yyyy-MM-dd}', " +
                         $"{this.Status}, {this.ProjectId})";

            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        // Обновляет существующую задачу
        public void Update()
        {
            string SQL = $"UPDATE `Tasks` SET " +
                        $"`name`='{this.Name}', " +
                        $"`description`='{this.Description}', " +
                        $"`dueDate`='{this.DueDate:yyyy-MM-dd}', " +
                        $"`status`={this.Status}, " +
                        $"`projectId`={this.ProjectId} " +
                        $"WHERE `id`={this.Id}";

            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        // Удаляет задачу из БД
        public void Delete()
        {
            string SQL = $"DELETE FROM `Tasks` WHERE `id`={this.Id}";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }
    }
}