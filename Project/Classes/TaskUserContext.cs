using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Classes
{
    public class TaskUserContext : TaskUser
    {
        public TaskUserContext(int Id, int UserId, int TaskId) : base(Id, UserId, TaskId) {}

        public static List<TaskUserContext> Get()
        {
            List<TaskUserContext> AllTaskUser = new List<TaskUserContext>();
            string SQL = $"SELECT * FROM `task_user`;";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader Data = Connection.Query(SQL, connection);
            while (Data.Read())
            {
                AllTaskUser.Add(new TaskUserContext(
                    Data.GetInt32(0),
                    Data.GetInt32(1),
                    Data.GetInt32(2)
                ));
            }
            Connection.CloseConnection(connection);
            return AllTaskUser;
        }
        public void Add()
        {
            string SQL = "INSERT INTO " +
                            "`task_user`(" +
                            "`id`, " +
                            "`user`, " +
                            "`task`) " +
                        "VALUES (" +
                            $"'{this.Id}'," +
                            $"'{this.UserId}'," +
                            $"'{this.TaskId}')";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        public void Update()
        {
            string SQL = "UPDATE " +
                            "`task_user` " +
                         "SET " +
                            $"`user`='{this.UserId}'," +
                            $"`task`='{this.TaskId}'" +
                         " WHERE " +
                            $"`id`='{this.Id}'";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        public void Delete()
        {
            string SQL = $"DELETE FROM `task_user` WHERE id={this.Id}";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }
    }
}
