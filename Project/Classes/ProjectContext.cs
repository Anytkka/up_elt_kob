using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Classes
{
    public class ProjectContext : Project
    {
        public ProjectContext(int Id, string Name, string Description, bool IsPublic) : base(Id, Name, Description, IsPublic) { }

        public static List<ProjectContext> Get()
        {
            List<ProjectContext> AllUser = new List<ProjectContext>();
            string SQL = $"SELECT * FROM `project`;";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader Data = Connection.Query(SQL, connection);
            while (Data.Read())
            {
                AllUser.Add(new ProjectContext(
                    Data.GetInt32(0),
                    Data.GetString(1),
                    Data.GetString(2),
                    Data.GetBoolean(3)
                ));
            }
            Connection.CloseConnection(connection);
            return AllUser;
        }
        public void Add()
        {
            string SQL = "INSERT INTO " +
                            "`project`( " +
                                "`id`, " +
                                "`name`, " +
                                "`description`, " +
                                "`is_public`) " +
                        "VALUES ( " +
                            $"'{this.Id}', " +
                            $"'{this.Name}', " +
                            $"'{this.Description}', " +
                            $"'{this.IsPublic}')";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }
        public void Update()
        {
            string SQL = "UPDATE " +
                            "`project` " +
                          "SET " +
                                $"`name`='{this.Name}', " +
                                $"`description`='{this.Description}', " +
                                $"`is_public`='{this.IsPublic}' " +
                          "WHERE " +
                                $"`id`='{this.Id}'";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }

        public void Delete()
        {
            string SQL = $"DELETE FROM `project` `id`='{this.Id}'";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }
    }
}