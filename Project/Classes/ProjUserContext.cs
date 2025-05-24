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

        public static List<ProjUserContext> Get()
        {
            List<ProjUserContext> allProjUsers = new List<ProjUserContext>();
            string SQL = "SELECT id, project, user, role FROM `project_user`";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    using (MySqlDataReader data = cmd.ExecuteReader())
                    {
                        while (data.Read())
                        {
                            allProjUsers.Add(new ProjUserContext(
                                data.GetInt32("id"),
                                data.GetInt32("project"),
                                data.GetInt32("user"),
                                data.GetString("role")
                            ));
                        }
                    }
                }
            }
            return allProjUsers;
        }

        public static ProjUserContext GetById(int id)
        {
            string SQL = "SELECT id, project, user, role FROM `project_user` WHERE id = @id";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (MySqlDataReader data = cmd.ExecuteReader())
                    {
                        if (data.Read())
                        {
                            return new ProjUserContext(
                                data.GetInt32("id"),
                                data.GetInt32("project"),
                                data.GetInt32("user"),
                                data.GetString("role")
                            );
                        }
                    }
                }
            }
            return null;
        }

        public static List<ProjUserContext> GetByProjectId(int projectId)
        {
            List<ProjUserContext> projUsers = new List<ProjUserContext>();
            string SQL = "SELECT id, project, user, role FROM `project_user` WHERE project = @projectId";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    cmd.Parameters.AddWithValue("@projectId", projectId);
                    using (MySqlDataReader data = cmd.ExecuteReader())
                    {
                        while (data.Read())
                        {
                            projUsers.Add(new ProjUserContext(
                                data.GetInt32("id"),
                                data.GetInt32("project"),
                                data.GetInt32("user"),
                                data.GetString("role")
                            ));
                        }
                    }
                }
            }
            return projUsers;
        }

        public static List<ProjUserContext> GetByUserId(int userId)
        {
            List<ProjUserContext> projUsers = new List<ProjUserContext>();
            string SQL = "SELECT id, project, user, role FROM `project_user` WHERE user = @userId";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (MySqlDataReader data = cmd.ExecuteReader())
                    {
                        while (data.Read())
                        {
                            projUsers.Add(new ProjUserContext(
                                data.GetInt32("id"),
                                data.GetInt32("project"),
                                data.GetInt32("user"),
                                data.GetString("role")
                            ));
                        }
                    }
                }
            }
            return projUsers;
        }

        public void Add()
        {
            string SQL = "INSERT INTO `project_user` (project, user, role) VALUES (@project, @user, @role)";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    cmd.Parameters.AddWithValue("@project", this.Project);
                    cmd.Parameters.AddWithValue("@user", this.User);
                    cmd.Parameters.AddWithValue("@role", this.Role);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "SELECT LAST_INSERT_ID()";
                    this.Id = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update()
        {
            string SQL = "UPDATE `project_user` SET project = @project, user = @user, role = @role WHERE id = @id";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    cmd.Parameters.AddWithValue("@project", this.Project);
                    cmd.Parameters.AddWithValue("@user", this.User);
                    cmd.Parameters.AddWithValue("@role", this.Role);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete()
        {
            string SQL = "DELETE FROM `project_user` WHERE id = @id";

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