using Project.Class.Project.Class;
using System;
using System.Collections.Generic;
using System.Data.OleDb;



namespace Project.Class
{
    public static class DBCconnect
    {
        public static OleDbConnection Connection()
        {
            string connectionString = "server=localhost;port=3307;database=progect;uid=root;pwd=;";
            return new OleDbConnection(connectionString);
        }

        public static OleDbDataReader Query(string sql, OleDbConnection connection)
        {
            OleDbCommand command = new OleDbCommand(sql, connection);
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();
            return command.ExecuteReader();
        }

        public static void CloseConnection(OleDbConnection connection)
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }
    }

    

    public class DocumentContext
    {
        //все задачи
        public List<Task> AllTasks()
        {
            List<Task> allTasks = new List<Task>();
            OleDbConnection connection = DBCconnect.Connection();
            OleDbDataReader dataTasks = DBCconnect.Query("SELECT * FROM [Tasks]", connection);

            while (dataTasks.Read())
            {
                Task newTask = new Task(
                    id: dataTasks.GetInt32(0),
                    name: dataTasks.GetString(1),
                    description: dataTasks.GetString(2),
                    dueDate: dataTasks.GetDateTime(3),
                    status: dataTasks.GetInt32(4),
                    projectId: dataTasks.GetInt32(5)
                );
                allTasks.Add(newTask);
            }

            DBCconnect.CloseConnection(connection);
            return allTasks;
        }

        //сохранение изменений задач
        public void Save(Task task, bool Update = false)
        {
            OleDbConnection connection = DBCconnect.Connection();

            if (Update)
            {
                DBCconnect.Query("UPDATE [Tasks] " +
                "SET " +
                $"[Name] = '{task.Name}', " +
                $"[Description] = '{task.Description}', " +
                $"[DueDate] = '{task.DueDate.ToString("dd.MM.yyyy")}', " +
                $"[Status] = {task.Status}, " +
                $"[ProjectId] = {task.ProjectId} " +
                $"WHERE [Id] = {task.Id}", connection);
            }
            else
            {
                DBCconnect.Query("INSERT INTO " +
                "[Tasks] " +
                "([Name], " +
                "[Description], " +
                "[DueDate], " +
                "[Status], " +
                "[ProjectId]) " +
                "VALUES (" +
                $"'{task.Name}', " +
                $"'{task.Description}', " +
                $"'{task.DueDate.ToString("dd.MM.yyyy")}', " +
                $"{task.Status}, " +
                $"{task.ProjectId})", connection);
            }

            DBCconnect.CloseConnection(connection);
        }

        //удаление задач
        public void Delete(int taskId)
        {
            OleDbConnection connection = DBCconnect.Connection();
            DBCconnect.Query($"DELETE FROM [Tasks] WHERE [Id] = {taskId}", connection);
            DBCconnect.CloseConnection(connection);
        }

        //задачи по iD
        public Task GetTaskById(int id)
        {
            OleDbConnection connection = DBCconnect.Connection();
            OleDbDataReader dataTask = DBCconnect.Query($"SELECT * FROM [Tasks] WHERE [Id] = {id}", connection);

            if (dataTask.Read())
            {
                Task task = new Task(
                    id: dataTask.GetInt32(0),
                    name: dataTask.GetString(1),
                    description: dataTask.GetString(2),
                    dueDate: dataTask.GetDateTime(3),
                    status: dataTask.GetInt32(4),
                    projectId: dataTask.GetInt32(5)
                );

                DBCconnect.CloseConnection(connection);
                return task;
            }

            DBCconnect.CloseConnection(connection);
            return null;
        }
    }

    public class SubtaskContext
    {
        //все подзадачи
        public List<Subtask> AllSubtasks()
        {
            List<Subtask> allSubtasks = new List<Subtask>();
            OleDbConnection connection = DBCconnect.Connection();
            OleDbDataReader dataSubtasks = DBCconnect.Query("SELECT * FROM [Subtasks]", connection);

            while (dataSubtasks.Read())
            {
                Subtask newSubtask = new Subtask(
                    id: dataSubtasks.GetInt32(0),
                    name: dataSubtasks.GetString(1),
                    description: dataSubtasks.GetString(2),
                    dueDate: dataSubtasks.GetDateTime(3),
                    status: dataSubtasks.GetInt32(4),
                    taskId: dataSubtasks.GetInt32(5)
                );
                allSubtasks.Add(newSubtask);
            }

            DBCconnect.CloseConnection(connection);
            return allSubtasks;
        }

        //сохранение изменений подзадач
        public void Save(Subtask subtask, bool Update = false)
        {
            OleDbConnection connection = DBCconnect.Connection();

            if (Update)
            {
                DBCconnect.Query("UPDATE [Subtasks] " +
                "SET " +
                $"[Name] = '{subtask.Name}', " +
                $"[Description] = '{subtask.Description}', " +
                $"[DueDate] = '{subtask.DueDate.ToString("dd.MM.yyyy")}', " +
                $"[Status] = {subtask.Status}, " +
                $"[TaskId] = {subtask.TaskId} " +
                $"WHERE [Id] = {subtask.Id}", connection);
            }
            else
            {
                DBCconnect.Query("INSERT INTO " +
                "[Subtasks] " +
                "([Name], " +
                "[Description], " +
                "[DueDate], " +
                "[Status], " +
                "[TaskId]) " +
                "VALUES (" +
                $"'{subtask.Name}', " +
                $"'{subtask.Description}', " +
                $"'{subtask.DueDate.ToString("dd.MM.yyyy")}', " +
                $"{subtask.Status}, " +
                $"{subtask.TaskId})", connection);
            }

            DBCconnect.CloseConnection(connection);
        }
        
        //удаление подзадач
        public void Delete(int subtaskId)
        {
            OleDbConnection connection = DBCconnect.Connection();
            DBCconnect.Query($"DELETE FROM [Subtasks] WHERE [Id] = {subtaskId}", connection);
            DBCconnect.CloseConnection(connection);
        }

        public Subtask GetSubtaskById(int id)
        {
            OleDbConnection connection = DBCconnect.Connection();
            OleDbDataReader dataSubtask = DBCconnect.Query($"SELECT * FROM [Subtasks] WHERE [Id] = {id}", connection);

            if (dataSubtask.Read())
            {
                Subtask subtask = new Subtask(
                    id: dataSubtask.GetInt32(0),
                    name: dataSubtask.GetString(1),
                    description: dataSubtask.GetString(2),
                    dueDate: dataSubtask.GetDateTime(3),
                    status: dataSubtask.GetInt32(4),
                    taskId: dataSubtask.GetInt32(5)
                );

                DBCconnect.CloseConnection(connection);
                return subtask;
            }

            DBCconnect.CloseConnection(connection);
            return null;
        }

        //все подзадачти к какой-то задаче
        public List<Subtask> GetSubtasksByTaskId(int taskId)
        {
            List<Subtask> subtasks = new List<Subtask>();
            OleDbConnection connection = DBCconnect.Connection();
            OleDbDataReader dataSubtasks = DBCconnect.Query($"SELECT * FROM [Subtasks] WHERE [TaskId] = {taskId}", connection);

            while (dataSubtasks.Read())
            {
                Subtask subtask = new Subtask(
                    id: dataSubtasks.GetInt32(0),
                    name: dataSubtasks.GetString(1),
                    description: dataSubtasks.GetString(2),
                    dueDate: dataSubtasks.GetDateTime(3),
                    status: dataSubtasks.GetInt32(4),
                    taskId: dataSubtasks.GetInt32(5)
                );
                subtasks.Add(subtask);
            }

            DBCconnect.CloseConnection(connection);
            return subtasks;
        }
    }

    public class ProjUserContext
    {
        // Все связи пользователей с проектами
        public List<ProjUser> AllProjUsers()
        {
            List<ProjUser> allProjUsers = new List<ProjUser>();
            OleDbConnection connection = DBCconnect.Connection();
            OleDbDataReader dataProjUsers = DBCconnect.Query("SELECT * FROM [ProjUsers]", connection);

            while (dataProjUsers.Read())
            {
                ProjUser newProjUser = new ProjUser(
                    id: dataProjUsers.GetInt32(0),
                    project: dataProjUsers.GetInt32(1),
                    user: dataProjUsers.GetInt32(2),
                    role: dataProjUsers.GetString(3)
                );
                allProjUsers.Add(newProjUser);
            }

            DBCconnect.CloseConnection(connection);
            return allProjUsers;
        }


        // Сохранение

        public void Save(ProjUser projUser, bool Update = false)
        {
            OleDbConnection connection = DBCconnect.Connection();

            if (Update)
            {
                DBCconnect.Query("UPDATE [ProjUsers] " +
                "SET " +
                $"[Project] = {projUser.Project}, " +
                $"[User] = {projUser.User}, " +
                $"[Role] = '{projUser.Role}' " +
                $"WHERE [Id] = {projUser.Id}", connection);
            }
            else
            {
                DBCconnect.Query("INSERT INTO " +
                "[ProjUsers] " +
                "([Project], " +
                "[User], " +
                "[Role]) " +
                "VALUES (" +
                $"{projUser.Project}, " +
                $"{projUser.User}, " +
                $"'{projUser.Role}')", connection);
            }

            DBCconnect.CloseConnection(connection);
        }

        // Удаление связи 
        public void Delete(int projUserId)
        {
            OleDbConnection connection = DBCconnect.Connection();
            DBCconnect.Query($"DELETE FROM [ProjUsers] WHERE [Id] = {projUserId}", connection);
            DBCconnect.CloseConnection(connection);
        }

        // Получение связи по ID
        public ProjUser GetProjUserById(int id)
        {
            OleDbConnection connection = DBCconnect.Connection();
            OleDbDataReader dataProjUser = DBCconnect.Query($"SELECT * FROM [ProjUsers] WHERE [Id] = {id}", connection);

            if (dataProjUser.Read())
            {
                ProjUser projUser = new ProjUser(
                    id: dataProjUser.GetInt32(0),
                    project: dataProjUser.GetInt32(1),
                    user: dataProjUser.GetInt32(2),
                    role: dataProjUser.GetString(3)
                );

                DBCconnect.CloseConnection(connection);
                return projUser; 
            }

            DBCconnect.CloseConnection(connection);
            return null;
        }

        // Получение всех связей для конкретного проекта
        public List<ProjUser> GetProjUsersByProjectId(int projectId)
        {
            List<ProjUser> projUsers = new List<ProjUser>();
            OleDbConnection connection = DBCconnect.Connection();
            OleDbDataReader dataProjUsers = DBCconnect.Query($"SELECT * FROM [ProjUsers] WHERE [Project] = {projectId}", connection);

            while (dataProjUsers.Read())
            {
                ProjUser projUser = new ProjUser(
                    id: dataProjUsers.GetInt32(0),
                    project: dataProjUsers.GetInt32(1),
                    user: dataProjUsers.GetInt32(2),
                    role: dataProjUsers.GetString(3)
                );
                projUsers.Add(projUser);
            }

            DBCconnect.CloseConnection(connection);
            return projUsers;
        }

        // Получение всех связей для конкретного пользователя
        public List<ProjUser> GetProjUsersByUserId(int userId)
        {
            List<ProjUser> projUsers = new List<ProjUser>();
            OleDbConnection connection = DBCconnect.Connection();
            OleDbDataReader dataProjUsers = DBCconnect.Query($"SELECT * FROM [ProjUsers] WHERE [User] = {userId}", connection);

            while (dataProjUsers.Read())
            {
                ProjUser projUser = new ProjUser(
                    id: dataProjUsers.GetInt32(0),
                    project: dataProjUsers.GetInt32(1),
                    user: dataProjUsers.GetInt32(2),
                    role: dataProjUsers.GetString(3)
                );
                projUsers.Add(projUser);
            }

            DBCconnect.CloseConnection(connection);
            return projUsers;
        }
    }
}



