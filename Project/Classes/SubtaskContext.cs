using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;

namespace Project.Classes
{
    public class SubtaskContext : Subtask
    {
        public SubtaskContext() : base(0, "", "", DateTime.Now, 0, 0, 0)
        {
        }

        public SubtaskContext(int id, string name, string description, DateTime dueDate, int taskId, int userId, int statusId)
            : base(id, name, description, dueDate, taskId, userId, statusId)
        {
        }

        // Получает все подзадачи
        public static List<SubtaskContext> Get()
        {
            List<SubtaskContext> allSubtasks = new List<SubtaskContext>();
            string SQL = "SELECT * FROM `Subtask`;";
            using (var connection = Connection.OpenConnection())
            {
                try
                {
                    using (var cmd = new MySqlCommand(SQL, connection))
                    {
                        using (MySqlDataReader data = cmd.ExecuteReader())
                        {
                            while (data.Read())
                            {
                                allSubtasks.Add(new SubtaskContext(
                                    data.GetInt32("id"),
                                    data.GetString("name"),
                                    data.GetString("description"),
                                    data.GetDateTime("due_date"),
                                    data.GetInt32("task_id"),
                                    data.GetInt32("user_id"),
                                    data.GetInt32("status_id")
                                ));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки всех подзадач: {ex.Message}");
                    throw;
                }
            }
            return allSubtasks;
        }

        // Получает подзадачу по ID
        public static SubtaskContext GetById(int id)
        {
            string SQL = "SELECT * FROM `Subtask` WHERE `id`=@id";
            using (var connection = Connection.OpenConnection())
            {
                try
                {
                    using (var cmd = new MySqlCommand(SQL, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader data = cmd.ExecuteReader())
                        {
                            if (data.Read())
                            {
                                var subtask = new SubtaskContext(
                                    data.GetInt32("id"),
                                    data.GetString("name"),
                                    data.GetString("description"),
                                    data.GetDateTime("due_date"),
                                    data.GetInt32("task_id"),
                                    data.GetInt32("user_id"),
                                    data.GetInt32("status_id")
                                );
                                return subtask;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки подзадачи с ID {id}: {ex.Message}");
                    throw;
                }
            }
            return null;
        }

        // Получает все подзадачи для конкретной задачи
        public static List<SubtaskContext> GetByTaskId(int taskId)
        {
            List<SubtaskContext> subtasks = new List<SubtaskContext>();
            string SQL = "SELECT * FROM `Subtask` WHERE `task_id`=@taskId";
            using (var connection = Connection.OpenConnection())
            {
                try
                {
                    using (var cmd = new MySqlCommand(SQL, connection))
                    {
                        cmd.Parameters.AddWithValue("@taskId", taskId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                subtasks.Add(new SubtaskContext(
                                    reader.GetInt32("id"),
                                    reader.GetString("name"),
                                    reader.GetString("description"),
                                    reader.GetDateTime("due_date"),
                                    reader.GetInt32("task_id"),
                                    reader.GetInt32("user_id"),
                                    reader.GetInt32("status_id")
                                ));
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"Загружено подзадач для задачи {taskId}: {subtasks.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки подзадач для задачи {taskId}: {ex.Message}");
                    throw;
                }
            }
            return subtasks;
        }

        // Добавляет новую подзадачу
        public void Add()
        {
            string SQL = "INSERT INTO `Subtask` (`name`, `description`, `due_date`, `task_id`, `user_id`, `status_id`) " +
                         "VALUES (@name, @description, @dueDate, @taskId, @userId, @statusId); SELECT LAST_INSERT_ID();";

            using (var connection = Connection.OpenConnection())
            {
                try
                {
                    using (var cmd = new MySqlCommand(SQL, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", this.Name);
                        cmd.Parameters.AddWithValue("@description", this.Description ?? string.Empty);
                        cmd.Parameters.AddWithValue("@dueDate", this.DueDate);
                        cmd.Parameters.AddWithValue("@taskId", this.TaskId);
                        cmd.Parameters.AddWithValue("@userId", this.UserId);
                        cmd.Parameters.AddWithValue("@statusId", this.StatusId);
                        this.Id = Convert.ToInt32(cmd.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Новая подзадача добавлена: ID={this.Id}, Name={this.Name}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка добавления подзадачи: {ex.Message}");
                    throw;
                }
            }
        }

        // Обновляет существующую подзадачу
        public void Update()
        {
            string SQL = "UPDATE `Subtask` SET " +
                         "`name`=@name, " +
                         "`description`=@description, " +
                         "`due_date`=@dueDate, " +
                         "`task_id`=@taskId, " +
                         "`user_id`=@userId, " +
                         "`status_id`=@statusId " +
                         "WHERE `id`=@id";

            using (var connection = Connection.OpenConnection())
            {
                try
                {
                    using (var cmd = new MySqlCommand(SQL, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", this.Id);
                        cmd.Parameters.AddWithValue("@name", this.Name);
                        cmd.Parameters.AddWithValue("@description", this.Description ?? string.Empty);
                        cmd.Parameters.AddWithValue("@dueDate", this.DueDate);
                        cmd.Parameters.AddWithValue("@taskId", this.TaskId);
                        cmd.Parameters.AddWithValue("@userId", this.UserId);
                        cmd.Parameters.AddWithValue("@statusId", this.StatusId);
                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Подзадача обновлена: ID={this.Id}, Name={this.Name}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка обновления подзадачи: {ex.Message}");
                    throw;
                }
            }
        }

        // Удаляет подзадачу
        public void Delete()
        {
            using (var connection = Connection.OpenConnection())
            {
                try
                {
                    const string SQL = "DELETE FROM Subtask WHERE id = @id";
                    using (var cmd = new MySqlCommand(SQL, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", this.Id);
                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Подзадача удалена: ID={this.Id}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка удаления подзадачи: {ex.Message}");
                    throw;
                }
            }
        }
    }
}