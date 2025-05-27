using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project.Classes;

namespace Project.Pages
{
    public partial class SubtaskDetailsPage : Page
    {
        private SubtaskContext _subtask;
        private List<UserContext> _responsibleUsers;

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
            // Загрузка данных подзадачи
            _subtask = SubtaskContext.GetById(subtaskId);

            // Загрузка ответственных пользователей
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
        }

        private string GetStatusName(int statusId)
        {
            // Логика получения имени статуса
            return $"Статус {statusId}";
        }

        private string GetTaskName(int taskId)
        {
            // Логика получения имени задачи
            var task = TaskContext.GetById(taskId);
            return task?.Name ?? "Не указан";
        }

       
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить эту подзадачу?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes && _subtask != null)
            {
                try
                {
                    _subtask.Delete();
                    MessageBox.Show("Подзадача успешно удалена");
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
