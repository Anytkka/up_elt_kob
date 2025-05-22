using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;

namespace Project.Classes
{
    public class DocumentContext : Task
    {
        public string ProjectCode { get; set; } // Код проекта из kanbanColumn
        public string ProjectName { get; set; } // Название проекта из Project

        public DocumentContext(int id, string name, string description, DateTime dueDate, int status, string projectCode, string projectName)
            : base(id, name, description, dueDate, status)
        {
            ProjectCode = projectCode;
            ProjectName = projectName;
        }

        public static List<DocumentContext> Get()
        {
            List<DocumentContext> allTasks = new List<DocumentContext>();
            string SQL = @"
                SELECT t.id, t.name, t.description, t.dueDate, t.status, 
                       kc.project AS projectCode, p.name AS projectName
                FROM `task` t
                LEFT JOIN `kanbanColumn` kc ON t.status = kc.id
                LEFT JOIN `Project` p ON kc.project = p.id";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (MySqlCommand command = new MySqlCommand(SQL, connection))
                {
                    using (MySqlDataReader data = command.ExecuteReader())
                    {
                        while (data.Read())
                        {
                            // Получаем projectCode как Int32 и преобразуем в строку
                            string projectCodeValue = data.IsDBNull(data.GetOrdinal("projectCode"))
                                ? ""
                                : data.GetInt32("projectCode").ToString();

                            allTasks.Add(new DocumentContext(
                                data.GetInt32("id"),
                                data.GetString("name"),
                                data.GetString("description"),
                                data.GetDateTime("dueDate"),
                                data.GetInt32("status"),
                                projectCodeValue,
                                data.IsDBNull(data.GetOrdinal("projectName")) ? "" : data.GetString("projectName")
                            ));
                        }
                    }
                }
            }
            return allTasks;
        }

        public static DocumentContext GetById(int id)
        {
            string SQL = @"
                SELECT t.id, t.name, t.description, t.dueDate, t.status, 
                       kc.project AS projectCode, p.name AS projectName
                FROM `task` t
                LEFT JOIN `kanbanColumn` kc ON t.status = kc.id
                LEFT JOIN `Project` p ON kc.project = p.id
                WHERE t.id = @id";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (MySqlCommand command = new MySqlCommand(SQL, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (MySqlDataReader data = command.ExecuteReader())
                    {
                        if (data.Read())
                        {
                            // Получаем projectCode как Int32 и преобразуем в строку
                            string projectCodeValue = data.IsDBNull(data.GetOrdinal("projectCode"))
                                ? ""
                                : data.GetInt32("projectCode").ToString();

                            return new DocumentContext(
                                data.GetInt32("id"),
                                data.GetString("name"),
                                data.GetString("description"),
                                data.GetDateTime("dueDate"),
                                data.GetInt32("status"),
                                projectCodeValue,
                                data.IsDBNull(data.GetOrdinal("projectName")) ? "" : data.GetString("projectName")
                            );
                        }
                    }
                }
            }
            return null;
        }

        public void Add()
        {
            string SQL = "INSERT INTO `task` (name, description, dueDate, status) " +
                        "VALUES (@name, @description, @dueDate, @status)";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (MySqlCommand command = new MySqlCommand(SQL, connection))
                {
                    command.Parameters.AddWithValue("@name", this.Name);
                    command.Parameters.AddWithValue("@description", this.Description);
                    command.Parameters.AddWithValue("@dueDate", this.DueDate);
                    command.Parameters.AddWithValue("@status", this.Status);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update()
        {
            if (Id <= 0)
            {
                throw new ArgumentException("ID задачи должен быть положительным числом.");
            }

            string SQL = "UPDATE `task` SET " +
                         "name = @name, " +
                         "description = @description, " +
                         "dueDate = @dueDate, " +
                         "status = @status " +
                         "WHERE id = @id";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                if (connection == null)
                {
                    throw new InvalidOperationException("Не удалось установить соединение с базой данных.");
                }

                using (MySqlCommand command = new MySqlCommand(SQL, connection))
                {
                    try
                    {
                        command.Parameters.AddWithValue("@name", this.Name ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@description", this.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@dueDate", this.DueDate != DateTime.MinValue ? this.DueDate : (object)DBNull.Value);
                        command.Parameters.AddWithValue("@status", this.Status);
                        command.Parameters.AddWithValue("@id", this.Id);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new InvalidOperationException($"Задача с ID {this.Id} не найдена в базе данных.");
                        }
                    }
                    catch (MySqlException ex)
                    {
                        throw new Exception($"Ошибка базы данных при обновлении задачи: {ex.Message}");
                    }
                }
            }
        }

        public void Delete()
        {
            string SQL = "DELETE FROM `task` WHERE id = @id";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (MySqlCommand command = new MySqlCommand(SQL, connection))
                {
                    command.Parameters.AddWithValue("@id", this.Id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}