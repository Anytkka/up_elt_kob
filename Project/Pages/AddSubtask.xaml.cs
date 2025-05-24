using Project.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using Project.Classes.Common;

namespace Project.Pages
{
    public partial class AddSubtask : Page
    {
        private int _taskId;
        private int _projectId;
        private SubtaskContext _editingSubtask;
        private List<UserContext> _responsiblePersons;

        public event Action<SubtaskContext> CreatedSubtask;
        public string ButtonText => _editingSubtask == null ? "Добавить" : "Обновить";

        public AddSubtask(int taskId, SubtaskContext subtask = null, int projectId = 0)
        {
            InitializeComponent();
            _taskId = taskId;
            _projectId = projectId;
            _editingSubtask = subtask;

            // Если projectId не передан и taskId не временный (не -1), извлекаем его из TaskContext
            if (_projectId == 0 && _taskId != -1)
            {
                var task = TaskContext.GetById(_taskId);
                if (task != null && int.TryParse(task.ProjectCode, out int extractedProjectId))
                {
                    _projectId = extractedProjectId;
                    Console.WriteLine($"Извлечён projectId: {_projectId} из задачи {_taskId}.");
                }
                else
                {
                    MessageBox.Show($"Не удалось определить ID проекта для задачи {_taskId}. Проверьте, существует ли задача.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.GoBack();
                    return;
                }
            }

            DataContext = this;
            LoadResponsiblePersons();
            InitializeFields();
        }

        private void LoadResponsiblePersons()
        {
            try
            {
                if (_projectId == 0)
                {
                    MessageBox.Show($"ID проекта не указан. Текущее значение: {_projectId}",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _responsiblePersons = new List<UserContext>();
                using (var connection = Connection.OpenConnection())
                {
                    if (connection == null)
                    {
                        MessageBox.Show("Не удалось подключиться к БД.", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string query = @"SELECT u.id, u.fullName
                           FROM user u
                           JOIN project_user pu ON u.id = pu.user
                           WHERE pu.project = @projectId";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", _projectId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string fullName = reader["fullName"]?.ToString() ?? "Не указано";
                                int userId = Convert.ToInt32(reader["id"]);
                                var user = new UserContext(userId, "", "", fullName, null, null);
                                _responsiblePersons.Add(user);
                                Console.WriteLine($"Загружен пользователь: ID = {userId}, FullName = {user.FullName}");
                            }
                        }
                    }
                }

                if (_responsiblePersons.Count == 0)
                {
                    MessageBox.Show($"Не найдено пользователей для проекта с ID {_projectId}. " +
                                    "Проверьте таблицу project_user и убедитесь, что для этого проекта есть пользователи.",
                                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    foreach (var user in _responsiblePersons)
                    {
                        if (string.IsNullOrEmpty(user.FullName))
                        {
                            Console.WriteLine($"Предупреждение: FullName пуст для пользователя ID = {user.Id}");
                            user.FullName = "Не указано";
                        }
                    }
                }

                Responsible.ItemsSource = null;
                Responsible.ItemsSource = _responsiblePersons;
                Responsible.DisplayMemberPath = "FullName";
                Responsible.SelectedValuePath = "Id";

                if (_responsiblePersons.Count > 0)
                {
                    Responsible.SelectedIndex = 0;
                    Console.WriteLine($"Выбран первый пользователь: {_responsiblePersons[0].FullName}");
                }
                else
                {
                    Console.WriteLine("Список пользователей пуст.");
                }

                Responsible.Items.Refresh();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка БД: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeFields()
        {
            if (_editingSubtask != null)
            {
                name.Text = _editingSubtask.Name;
                description.Text = _editingSubtask.Description;

                var responsible = _responsiblePersons.FirstOrDefault(u => u.Id == _editingSubtask.UserId);
                if (responsible != null)
                {
                    Responsible.SelectedItem = responsible;
                    Console.WriteLine($"Установлен ответственный: {responsible.FullName}");
                }
                else if (_responsiblePersons.Count > 0)
                {
                    Responsible.SelectedIndex = 0;
                    Console.WriteLine($"Ответственный не найден, выбран первый: {_responsiblePersons[0].FullName}");
                }
            }
            else
            {
                description.Text = "Опишите подробности";
                description.Foreground = Brushes.Gray;
            }
        }

        private void Bt_Cancel(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Bt_AddS(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(name.Text))
            {
                MessageBox.Show("Введите наименование подзадачи.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedUser = Responsible.SelectedItem as UserContext;
                if (selectedUser == null)
                {
                    MessageBox.Show("Выберите ответственного.", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создаем подзадачу без сохранения в базе, если задача еще не создана (_taskId = 0 или -1)
                SubtaskContext subtask;
                if (_editingSubtask == null)
                {
                    subtask = new SubtaskContext(
                        0, // Временный ID, будет обновлен при сохранении основной задачи
                        name.Text,
                        description.Text == "Опишите подробности" ? "" : description.Text,
                        DateTime.Now.AddDays(7),
                        _taskId, // Передаем _taskId как есть, даже если 0 или -1
                        selectedUser.Id,
                        GetNewColumnId()
                    );

                    // Не вызываем subtask.Add() здесь, так как сохранение произойдет после создания задачи
                }
                else
                {
                    subtask = _editingSubtask;
                    subtask.Name = name.Text;
                    subtask.Description = description.Text == "Опишите подробности" ? "" : description.Text;
                    subtask.UserId = selectedUser.Id;
                    subtask.Update(); // Обновляем в базе данных, если редактируем
                }

                CreatedSubtask?.Invoke(subtask);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подготовке подзадачи: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetNewColumnId()
        {
            if (_taskId != -1 && _taskId > 0)
            {
                var task = TaskContext.GetById(_taskId);
                if (task != null && int.TryParse(task.ProjectCode, out int projectId))
                {
                    return KanbanColumnContext.Get()
                        .FirstOrDefault(c => c.ProjectId == projectId && c.TitleStatus == "Новые")?.Id ?? 1;
                }
            }
            return 1; // По умолчанию возвращаем 1, если задача еще не создана
        }
    }
}