using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project.Classes;

namespace Project.Pages
{
    public partial class TaskDetailsPage : Page
    {
        private TaskContext _task;
        private List<UserContext> _responsibleUsers;

        public int TaskNumber => _task?.Id ?? 0;
        public string TaskName => _task?.Name ?? "Не указано";
        public string Description => _task?.Description ?? "Описание отсутствует";
        public DateTime DueDate => _task?.DueDate ?? DateTime.MinValue;
        public string StatusName => GetStatusName(_task?.Status ?? 0);
        public string ProjectName => _task?.ProjectName ?? "Не указан";
        public string ProjectCode => _task?.ProjectCode ?? "N/A";
        public List<UserContext> ResponsibleUsers => _responsibleUsers;

        public TaskDetailsPage(int taskId)
        {
            InitializeComponent();
            LoadTaskData(taskId);
            DataContext = this;
        }

        private void LoadTaskData(int taskId)
        {
            // Загрузка данных задачи
            _task = TaskContext.GetById(taskId);

            // Загрузка ответственных пользователей
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

        private string GetStatusName(int statusId)
        {
            // Здесь должна быть ваша логика получения имени статуса
            // Например, из базы данных или словаря
            return $"Статус {statusId}";
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
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
                    MessageBox.Show("Задача успешно удалена");
                    NavigationService.GoBack();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        private void Bt8_Cancel(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
