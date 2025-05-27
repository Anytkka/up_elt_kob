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
        private int _taskId; // Сохраняем taskId

        public int TaskNumber => _task?.Id ?? 0;
        public string TaskName => _task?.Name ?? "Не указано";
        public string Description => _task?.Description ?? "Описание отсутствует";
        public DateTime DueDate => _task?.DueDate ?? DateTime.MinValue;
        public string StatusName => GetStatusName(_task?.Status ?? 0);
        public string ProjectName => _task?.ProjectName ?? "Не указан";
        // Удаляем ProjectId, так как используем только ProjectCode
        public List<UserContext> ResponsibleUsers => _responsibleUsers;
        public bool CanDelete => _userRole == "Создатель" || _userRole == "Администратор";

        public TaskDetails(int taskId)
        {
            InitializeComponent();
            _taskId = taskId; // Сохраняем taskId
            LoadTaskData(taskId); // Сначала загружаем задачу
            LoadUserRole(); // Затем загружаем роль
            DataContext = this;
        }

        private void LoadUserRole()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    // Используем ProjectCode из TaskContext
                    int projectId = 0;
                    if (_task != null && !string.IsNullOrEmpty(_task.ProjectCode))
                    {
                        projectId = int.Parse(_task.ProjectCode); // Преобразуем ProjectCode в int
                    }
                    else
                    {
                        // Если ProjectCode пустой, пытаемся получить projectId через запрос
                        string projectQuery = @"
                            SELECT kc.project 
                            FROM project_task t
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
                var taskUsers = TaskUserContext.Get().Where(tu => tu.TaskId == taskId).ToList();
                var allUsers = UserContext.Get();

                foreach (var taskUser in taskUsers)
                {
                    var user = allUsers.FirstOrDefault(u => u.Id == taskUser.UserId);
                    if (user != null)
                    {
                        _responsibleUsers.Add(user);
                    }
                }
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDelete)
            {
                MessageBox.Show("Только Создатель и Администратор могут удалять задачи.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить эту задачу?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes && _task != null)
            {
                try
                {
                    _task.Delete();
                    MessageBox.Show("Задача успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService?.GoBack();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}