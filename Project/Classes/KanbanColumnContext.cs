using MySql.Data.MySqlClient;
using Project.Classes.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Classes
{
    public class KanbanColumnContext : KanbanColumn
    {
        public KanbanColumnContext(int Id, string TitleStatus, int ProjectId) : base(Id, TitleStatus, ProjectId) { }

        public static List<KanbanColumnContext> Get()
        {
            List<KanbanColumnContext> AllUser = new List<KanbanColumnContext>();
            string SQL = $"SELECT * FROM `kanbanColumn`;";
            MySqlConnection connection = Connection.OpenConnection();
            MySqlDataReader Data = Connection.Query(SQL, connection);
            while (Data.Read())
            {
                AllUser.Add(new KanbanColumnContext(
                    Data.GetInt32(0),
                    Data.GetString(1),
                    Data.GetInt32(2)
                ));
            }
            Connection.CloseConnection(connection);
            return AllUser;
        }
        public void Add()
        {
            string SQL = "INSERT INTO " +
                            "`kanbanColumn`(" +
                            "`id`, " +
                            "`title_status`, " +
                            "`project`) " +
                            "VALUES ('" +
                            $"{this.Id}'," +
                            $"'{this.TitleStatus}'," +
                            $"'{this.ProjectId}')";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }
        public void Update()
        {
            string SQL = "UPDATE " +
                            "`kanbanColumn` " +
                         "SET " +
                            $"`title_status`='{this.TitleStatus}'," +
                            $"`project`='{this.ProjectId}' " +
                         "WHERE " +
                            $"`id`='{this.Id}'";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }
        public void Delete()
        {
            string SQL = $"DELETE FROM `kanbanColumn` `id`='{this.Id}'";
            MySqlConnection connection = Connection.OpenConnection();
            Connection.Query(SQL, connection);
            Connection.CloseConnection(connection);
        }
    }
}
