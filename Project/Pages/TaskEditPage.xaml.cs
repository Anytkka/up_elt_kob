using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project.Classes;
using MySql.Data.MySqlClient;
using Project.Classes.Common;

namespace Project.Pages
{
    public partial class TaskEditPage : Page
    {
        private TaskContext _task;
        private List<UserSelection> _allUsers;
        private List<Status> _statuses;
        private List<Project> _projects;

        public int TaskNumber => _task?.Id ?? 0;
        public string TaskName { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int Status { get; set; }
        public int ProjectId { get; set; }
        public List<UserSelection> AllUsers => _allUsers;
        public List<Status> Statuses => _statuses;
        public List<Project> Projects => _projects;

        public TaskEditPage(int taskId)
        {
            InitializeComponent();
            LoadData(taskId);
            DataContext = this;
        }

        private void LoadData(int taskId)
        {
            // Загрузка задачи
            _task = TaskContext.GetById(taskId);
            if (_task != null)
            {
                TaskName = _task.Name;
                Description = _task.Description;
                DueDate = _task.DueDate;
                Status = _task.Status;
                ProjectId = int.TryParse(_task.ProjectCode, out var id) ? id : 0;
            }

          

            // Загрузка проектов
            _projects = Project.GetAllProjects();

            // Загрузка пользователей
            var allUsers = UserContext.Get();
            var responsibleUsers = TaskUserContext.Get()
                .Where(tu => tu.TaskId == _task.Id)
                .Select(tu => tu.UserId)
                .ToList();

            _allUsers = allUsers.Select(u => new UserSelection
            {
                User = u,
                IsSelected = responsibleUsers.Contains(u.Id)
            }).ToList();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновление данных задачи
                _task.Name = TaskName;
                _task.Description = Description;
                _task.DueDate = DueDate;
                _task.Status = Status;
                _task.Update();

                // Обновление ответственных пользователей
                UpdateResponsibleUsers();

                MessageBox.Show("Задача успешно сохранена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.GoBack();
                
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateResponsibleUsers()
        {
         
            var currentResponsible = TaskUserContext.Get()
                .Where(tu => tu.TaskId == _task.Id)
                .ToList();

            
            foreach (var taskUser in currentResponsible)
            {
                if (!_allUsers.Any(u => u.IsSelected && u.User.Id == taskUser.UserId))
                {
                    taskUser.Delete();
                }
            }

            
            foreach (var userSelection in _allUsers.Where(u => u.IsSelected))
            {
                if (!currentResponsible.Any(tu => tu.UserId == userSelection.User.Id))
                {
                    var newTaskUser = new TaskUserContext(0, userSelection.User.Id, _task.Id);
                    newTaskUser.Add();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }

    public class UserSelection
    {
        public UserContext User { get; set; }
        public bool IsSelected { get; set; }
    }

    public class Status
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static List<Status> GetAllStatuses()
        {
            var statuses = new List<Status>();
            var connection = Connection.OpenConnection();
            try
            {
                string query = "SELECT id, name FROM statuses";
                var command = new MySqlCommand(query, connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    statuses.Add(new Status
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name")
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Connection.CloseConnection(connection);
            }
            return statuses;
        }
    }

    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static List<Project> GetAllProjects()
        {
            var projects = new List<Project>();
            var connection = Connection.OpenConnection();
            try
            {
                string query = "SELECT id, name FROM projects";
                var command = new MySqlCommand(query, connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    projects.Add(new Project
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name")
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке проектов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Connection.CloseConnection(connection);
            }
            return projects;
        }
    }
}
