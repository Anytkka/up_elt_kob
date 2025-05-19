using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;

namespace Project.Classes
{
    public class SubtaskContext : Subtask
    {
        public SubtaskContext(int id, string name, string description, DateTime dueDate, int status, int taskId)
            : base(id, name, description, dueDate, status, taskId) { }

        // Получает все подзадачи
        public static List<SubtaskContext> Get()
        {
            List<SubtaskContext> allSubtasks = new List<SubtaskContext>();
            string SQL = "SELECT * FROM `Subtasks`;";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader data = Connection.Query(SQL, connection);

            while (data.Read())
            {
                allSubtasks.Add(new SubtaskContext(
                    data.GetInt32(0),   
                    data.GetString(1),   
                    data.GetString(2),   
                    data.GetDateTime(3), 
                    data.GetInt32(4),    
                    data.GetInt32(5)     
                ));
            }

            Connection.CloseConnection(connection);
            return allSubtasks;
        }

        // Получает подзадачу по ID
        public static SubtaskContext GetById(int id)
        {
            string SQL = $"SELECT * FROM `Subtasks` WHERE `id`='{id}'";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader data = Connection.Query(SQL, connection);

            if (data.Read())
            {
                var subtask = new SubtaskContext(
                    data.GetInt32(0),
                    data.GetString(1),
                    data.GetString(2),
                    data.GetDateTime(3),
                    data.GetInt32(4),
                    data.GetInt32(5)
                );
                Connection.CloseConnection(connection);
                return subtask;
            }

            Connection.CloseConnection(connection);
            return null;
        }

        // Получает все подзадачи для конкретной задачи
        public static List<SubtaskContext> GetByTaskId(int taskId)
        {
            List<SubtaskContext> subtasks = new List<SubtaskContext>();
            string SQL = $"SELECT * FROM `Subtasks` WHERE `taskId`='{taskId}'";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader data = Connection.Query(SQL, connection);

            while (data.Read())
            {
                subtasks.Add(new SubtaskContext(
                    data.GetInt32(0),
                    data.GetString(1),
                    data.GetString(2),
                    data.GetDateTime(3),
                    data.GetInt32(4),
                    data.GetInt32(5)
                ));
            }

            Connection.CloseConnection(connection);
            return subtasks;
        }

        // Добавляет новую подзадачу
        public void Add()
        {
            string SQL = $"INSERT INTO `Subtasks` (`name`, `description`, `dueDate`, `status`, `taskId`) " +
                         $"VALUES ('{this.Name}', '{this.Description}', '{this.DueDate:yyyy-MM-dd}', " +
                         $"{this.Status}, {this.TaskId})";

            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        // Обновляет существующую подзадачу
        public void Update()
        {
            string SQL = $"UPDATE `Subtasks` SET " +
                        $"`name`='{this.Name}', " +
                        $"`description`='{this.Description}', " +
                        $"`dueDate`='{this.DueDate:yyyy-MM-dd}', " +
                        $"`status`={this.Status}, " +
                        $"`taskId`={this.TaskId} " +
                        $"WHERE `id`={this.Id}";

            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        // Удаляет подзадачу 
        public void Delete()
        {
            string SQL = $"DELETE FROM `Subtasks` WHERE `id`={this.Id}";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }
    }
}