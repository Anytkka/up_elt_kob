using Project.Classes;
using Project.Classes.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Project.Pages
{
    public partial class TaskDetails : Page
    {
        private TaskContext _task;
        private List<UserContext> _responsibleUsers;
        private string _userRole;
        private int _taskId; 

        public int TaskNumber => _task?.Id ?? 0;
        public string TaskName => _task?.Name ?? "Не указано";
        public string Description => _task?.Description ?? "Описание отсутствует";
        public DateTime DueDate => _task?.DueDate ?? DateTime.MinValue;
        public string StatusName => GetStatusName(_task?.Status ?? 0);
        public string ProjectName => _task?.ProjectName ?? "Не указан";
        public List<UserContext> ResponsibleUsers
        {
            get => _responsibleUsers;
            set
            {
                _responsibleUsers = value;
                listViewResponsible.ItemsSource = _responsibleUsers;
            }
        }

        public TaskDetails(int taskId)
        {
            InitializeComponent();
            _taskId = taskId;
            LoadTaskData(taskId);
            LoadUserRole();
            DataContext = this;
        }

        private void LoadUserRole()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    int projectId = 0;
                    if (_task != null && !string.IsNullOrEmpty(_task.ProjectCode))
                    {
                        projectId = int.Parse(_task.ProjectCode);
                    }
                    else
                    {
                        string projectQuery = @"
                            SELECT kc.project 
                            FROM task t
                            JOIN kanbanColumn kc ON t.status = kc.id
                            WHERE t.id = @taskId";
                        using (var cmd = new MySqlCommand(projectQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@taskId", _taskId);
                            var result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                projectId = Convert.ToInt32(result);
                            }
                        }
                    }

                    if (projectId > 0)
                    {
                        string roleQuery = "SELECT role FROM project_user WHERE project = @projectId AND user = @userId";
                        using (var cmd = new MySqlCommand(roleQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@projectId", projectId);
                            cmd.Parameters.AddWithValue("@userId", App.CurrentUser?.Id ?? 0);
                            var result = cmd.ExecuteScalar();
                            _userRole = result?.ToString() ?? "Пользователь";
                        }
                    }
                    else
                    {
                        _userRole = "Пользователь";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке роли пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _userRole = "Пользователь";
            }
        }

        private void LoadTaskData(int taskId)
        {
            try
            {
                _task = TaskContext.GetById(taskId);
                _responsibleUsers = new List<UserContext>();

                using (var connection = Connection.OpenConnection())
                {
                    string responsibleQuery = @"
                        SELECT u.id, u.fullName
                        FROM task_user tu
                        JOIN user u ON tu.user = u.id
                        WHERE tu.task = @taskId";
                    using (var cmd = new MySqlCommand(responsibleQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@taskId", taskId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _responsibleUsers.Add(new UserContext(
                                    reader.GetInt32("id"),
                                    "",
                                    "",
                                    reader.GetString("fullName"),
                                    null,
                                    null
                                ));
                            }
                        }
                    }
                }
                System.Diagnostics.Debug.WriteLine($"TaskDetails: Loaded {_responsibleUsers.Count} responsible users for task ID {taskId}");
                ResponsibleUsers = _responsibleUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetStatusName(int statusId)
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    string query = "SELECT title_status FROM project_kanbanColumn WHERE id = @statusId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@statusId", statusId);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString() ?? $"Статус {statusId}";
                    }
                }
            }
            catch (Exception)
            {
                return $"Статус {statusId}";
            }
        }

        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}