using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;

namespace Project.Classes
{
    public class TaskUserContext : TaskUser
    {
        public TaskUserContext(int Id, int UserId, int TaskId) : base(Id, UserId, TaskId) { }

        public static List<TaskUserContext> Get()
        {
            List<TaskUserContext> allTaskUsers = new List<TaskUserContext>();
            string SQL = "SELECT id, user, task FROM `task_user`";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    using (MySqlDataReader data = cmd.ExecuteReader())
                    {
                        while (data.Read())
                        {
                            allTaskUsers.Add(new TaskUserContext(
                                data.GetInt32("id"),
                                data.GetInt32("user"),
                                data.GetInt32("task")
                            ));
                        }
                    }
                }
            }
            return allTaskUsers;
        }

        public void Add()
        {
            string SQL = "INSERT INTO `task_user` (user, task) VALUES (@user, @task)";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    cmd.Parameters.AddWithValue("@user", this.UserId);
                    cmd.Parameters.AddWithValue("@task", this.TaskId);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "SELECT LAST_INSERT_ID()";
                    this.Id = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Update()
        {
            string SQL = "UPDATE `task_user` SET user = @user, task = @task WHERE id = @id";

            using (MySqlConnection connection = Connection.OpenConnection())
            {
                using (var cmd = new MySqlCommand(SQL, connection))
                {
                    cmd.Parameters.AddWithValue("@id", this.Id);
                    cmd.Parameters.AddWithValue("@user", this.UserId);
                    cmd.Parameters.AddWithValue("@task", this.TaskId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete()
        {
            string SQL = "DELETE FROM `task_user` WHERE id = @id";

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