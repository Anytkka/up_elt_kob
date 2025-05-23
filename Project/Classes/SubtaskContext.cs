using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;

namespace Project.Classes
{
    public class SubtaskContext : Subtask
    {
        public SubtaskContext(int id, string name, string description, DateTime dueDate, int taskId, int userId, int statusId)
            : base(id, name, description, dueDate, taskId, userId, statusId) { }

        // Получает все подзадачи
        public static List<SubtaskContext> Get()
        {
            List<SubtaskContext> allSubtasks = new List<SubtaskContext>();
            string SQL = "SELECT * FROM `Subtask`;";
            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                using (MySqlDataReader data = cmd.ExecuteReader())
                {
                    while (data.Read())
                    {
                        allSubtasks.Add(new SubtaskContext(
                            data.GetInt32(0),
                            data.GetString(1),
                            data.GetString(2),
                            data.GetDateTime(3),
                            data.GetInt32(4),
                            data.GetInt32(5),
                            data.GetInt32(6)
                        ));
                    }
                }
            }
            Connection.CloseConnection(connection);
            return allSubtasks;
        }

        // Получает подзадачу по ID
        public static SubtaskContext GetById(int id)
        {
            string SQL = "SELECT * FROM `Subtask` WHERE `id`=@id";
            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (MySqlDataReader data = cmd.ExecuteReader())
                {
                    if (data.Read())
                    {
                        var subtask = new SubtaskContext(
                            data.GetInt32(0), 
                            data.GetString(1),
                            data.GetString(2),
                            data.GetDateTime(3),
                            data.GetInt32(4),
                            data.GetInt32(5),
                            data.GetInt32(6)
                        );
                        Connection.CloseConnection(connection);
                        return subtask;
                    }
                }
            }
            Connection.CloseConnection(connection);
            return null;
        }

        // Получает все подзадачи для конкретной задачи
        public static List<SubtaskContext> GetByTaskId(int taskId)
        {
            List<SubtaskContext> subtasks = new List<SubtaskContext>();
            string SQL = "SELECT * FROM `Subtask` WHERE `task`=@taskId";
            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                cmd.Parameters.AddWithValue("@taskId", taskId);
                using (MySqlDataReader data = cmd.ExecuteReader())
                {
                    while (data.Read())
                    {
                        subtasks.Add(new SubtaskContext(
                            data.GetInt32(0),  
                            data.GetString(1),  
                            data.GetString(2), 
                            data.GetDateTime(3), 
                            data.GetInt32(4),
                            data.GetInt32(5), 
                            data.GetInt32(6) 
                        ));
                    }
                }
            }
            Connection.CloseConnection(connection);
            return subtasks;
        }

        // Добавляет новую подзадачу
        public void Add()
        {
            string SQL = "INSERT INTO `Subtask` (`name`, `description`, `dueDate`, `task`, `user`, `status`) " +
                         "VALUES (@name, @description, @dueDate, @taskId, @userId, @statusId)";

            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                cmd.Parameters.AddWithValue("@name", this.Name);
                cmd.Parameters.AddWithValue("@description", this.Description);
                cmd.Parameters.AddWithValue("@dueDate", this.DueDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@taskId", this.TaskId);
                cmd.Parameters.AddWithValue("@userId", this.UserId);
                cmd.Parameters.AddWithValue("@statusId", this.StatusId);
                cmd.ExecuteNonQuery();
            }
            Connection.CloseConnection(connection);
        }

        // Обновляет существующую подзадачу
        public void Update()
        {
            string SQL = "UPDATE `Subtask` SET " +
                         "`name`=@name, " +
                         "`description`=@description, " +
                         "`dueDate`=@dueDate, " +
                         "`task`=@taskId, " +
                         "`user`=@userId, " +
                         "`status`=@statusId " +
                         "WHERE `id`=@id";

            MySqlConnection connection = Connection.OpenConnection();
            using (var cmd = new MySqlCommand(SQL, connection))
            {
                cmd.Parameters.AddWithValue("@id", this.Id);
                cmd.Parameters.AddWithValue("@name", this.Name);
                cmd.Parameters.AddWithValue("@description", this.Description);
                cmd.Parameters.AddWithValue("@dueDate", this.DueDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@taskId", this.TaskId);
                cmd.Parameters.AddWithValue("@userId", this.UserId);
                cmd.Parameters.AddWithValue("@statusId", this.StatusId);
                cmd.ExecuteNonQuery();
            }
            Connection.CloseConnection(connection);
        }

        // Удаляет подзадачу
        public void Delete()
        {
            string SQL = "DELETE FROM `Subtask` WHERE `id`=@id";
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