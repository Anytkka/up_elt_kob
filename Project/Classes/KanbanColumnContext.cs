using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;

namespace Project.Classes
{
    public class KanbanColumnContext : KanbanColumn
    {
        public KanbanColumnContext(int id, string titleStatus, int projectId) : base(id, titleStatus, projectId) { }

        public static List<KanbanColumnContext> Get()
        {
            List<KanbanColumnContext> allColumns = new List<KanbanColumnContext>();
            string SQL = "SELECT id, title_status, project FROM `kanbanColumn`";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (MySqlCommand command = new MySqlCommand(SQL, connection))
                {
                    using (MySqlDataReader data = command.ExecuteReader())
                    {
                        while (data.Read())
                        {
                            allColumns.Add(new KanbanColumnContext(
                                data.GetInt32("id"),
                                data.GetString("title_status"),
                                data.GetInt32("project")
                            ));
                        }
                    }
                }
            }
            Console.WriteLine($"Loaded {allColumns.Count} columns from kanbanColumn");
            return allColumns;
        }

        public void Add()
        {
            string SQL = "INSERT INTO `kanbanColumn` (title_status, project) VALUES (@titleStatus, @project)";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (MySqlCommand command = new MySqlCommand(SQL, connection))
                {
                    command.Parameters.AddWithValue("@titleStatus", this.TitleStatus);
                    command.Parameters.AddWithValue("@project", this.ProjectId);
                    command.ExecuteNonQuery();
                    command.CommandText = "SELECT LAST_INSERT_ID()";
                    this.Id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void Update()
        {
            string SQL = "UPDATE `kanbanColumn` SET title_status = @titleStatus, project = @project WHERE id = @id";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (MySqlCommand command = new MySqlCommand(SQL, connection))
                {
                    command.Parameters.AddWithValue("@titleStatus", this.TitleStatus);
                    command.Parameters.AddWithValue("@project", this.ProjectId);
                    command.Parameters.AddWithValue("@id", this.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete()
        {
            string SQL = "DELETE FROM `kanbanColumn` WHERE id = @id";

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