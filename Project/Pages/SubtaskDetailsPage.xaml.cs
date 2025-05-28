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
    public partial class SubtaskDetailsPage : Page
    {
        private SubtaskContext _subtask;
        private List<UserContext> _responsibleUsers;
        private int _projectId;

        public int SubtaskNumber => _subtask?.Id ?? 0;
        public string SubtaskName => _subtask?.Name ?? "Не указано";
        public string Description => _subtask?.Description ?? "Описание отсутствует";
        public DateTime DueDate => _subtask?.DueDate ?? DateTime.MinValue;
        public string StatusName => GetStatusName(_subtask?.StatusId ?? 0);
        public string TaskName => GetTaskName(_subtask?.TaskId ?? 0);
        public List<UserContext> ResponsibleUsers => _responsibleUsers;

        public SubtaskDetailsPage(int subtaskId)
        {
            InitializeComponent();
            LoadSubtaskData(subtaskId);
            DataContext = this;
        }

        private void LoadSubtaskData(int subtaskId)
        {
            try
            {
                _subtask = SubtaskContext.GetById(subtaskId);
                if (_subtask == null)
                {
                    MessageBox.Show("Подзадача не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.GoBack();
                    return;
                }
                _responsibleUsers = new List<UserContext>();
                var taskUsers = TaskUserContext.Get().Where(tu => tu.TaskId == _subtask.TaskId).ToList();
                var allUsers = UserContext.Get();

                foreach (var taskUser in taskUsers)
                {
                    var user = allUsers.FirstOrDefault(u => u.Id == taskUser.UserId);
                    if (user != null)
                    {
                        _responsibleUsers.Add(user);
                    }
                }

                listViewResponsible.ItemsSource = _responsibleUsers;

                // Определение projectId через задачу
                var task = TaskContext.GetById(_subtask.TaskId);
                if (task != null && int.TryParse(task.ProjectCode, out int projectId))
                {
                    _projectId = projectId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных подзадачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetStatusName(int statusId)
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    string query = "SELECT title_status FROM kanbanColumn WHERE id = @statusId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@statusId", statusId);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString() ?? $"Статус {statusId}";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching status name for ID {statusId}: {ex.Message}");
                return $"Статус {statusId}";
            }
        }

        private string GetTaskName(int taskId)
        {
            var task = TaskContext.GetById(taskId);
            return task?.Name ?? "Не указан";
        }

        private void Bt8_Cancel(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}