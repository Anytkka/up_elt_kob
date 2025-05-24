using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;

namespace Project.Classes
{
    public class ProjectContext : Project
    {
        public ProjectContext(int Id, string Name, string Description, bool IsPublic) : base(Id, Name, Description, IsPublic) { }

        public static List<ProjectContext> Get()
        {
            List<ProjectContext> allProjects = new List<ProjectContext>();
            string SQL = "SELECT id, name, description, is_public FROM `project`";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    using (MySqlDataReader data = cmd.ExecuteReader())
                    {
                        while (data.Read())
                        {
                            allProjects.Add(new ProjectContext(
                                data.GetInt32("id"),
                                data.GetString("name"),
                                data.GetString("description"),
                                data.GetBoolean("is_public")
                            ));
                        }
                    }
                }
            }
            return allProjects;
        }

        public void Add()
        {
            string SQL = "INSERT INTO `project` (name, description, is_public) VALUES (@name, @description, @isPublic)";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    cmd.Parameters.AddWithValue("@name", this.Name);
                    cmd.Parameters.AddWithValue("@description", this.Description);
                    cmd.Parameters.AddWithValue("@isPublic", this.IsPublic ? 1 : 0);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "SELECT LAST_INSERT_ID()";
                    this.Id = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update()
        {
            string SQL = "UPDATE `project` SET name = @name, description = @description, is_public = @isPublic WHERE id = @id";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    cmd.Parameters.AddWithValue("@name", this.Name);
                    cmd.Parameters.AddWithValue("@description", this.Description);
                    cmd.Parameters.AddWithValue("@isPublic", this.IsPublic ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete()
        {
            string SQL = "DELETE FROM `project` WHERE id = @id";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}