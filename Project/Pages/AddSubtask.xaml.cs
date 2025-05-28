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
        private string _userRole;

        public event Action<SubtaskContext> CreatedSubtask;
        public string ButtonText => _editingSubtask == null ? "Добавить" : "Обновить";
        public bool CanEdit => _userRole == "Создатель" || _userRole == "Администратор";

        public AddSubtask(int taskId, SubtaskContext subtask = null, int projectId = 0)
        {
            InitializeComponent();
            _taskId = taskId;
            _projectId = projectId;
            _editingSubtask = subtask;

            if (_projectId == 0 && _taskId != -1)
            {
                var task = TaskContext.GetById(_taskId);
                if (task != null && int.TryParse(task.ProjectCode, out int extractedProjectId))
                {
                    _projectId = extractedProjectId;
                    System.Diagnostics.Debug.WriteLine($"Извлечён projectId: {_projectId} из задачи {_taskId}.");
                }
                else
                {
                    MessageBox.Show($"Не удалось определить ID проекта для задачи {_taskId}. Проверьте, существует ли задача.",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.GoBack();
                    return;
                }
            }

            LoadUserRole();
            LoadResponsiblePersons();
            InitializeFields();
            DataContext = this;
        }

        private void LoadUserRole()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Загрузка роли пользователя. ProjectId: {_projectId}, UserId: {App.CurrentUser?.Id}");
                if (App.CurrentUser == null || App.CurrentUser.Id == 0)
                {
                    MessageBox.Show("Текущий пользователь не определён. Проверьте авторизацию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _userRole = "Пользователь";
                    return;
                }

                using (var connection = Connection.OpenConnection())
                {
                    if (connection == null)
                    {
                        MessageBox.Show("Не удалось подключиться к базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        _userRole = "Пользователь";
                        return;
                    }

                    string roleQuery = "SELECT role FROM project_user WHERE project = @projectId AND user = @userId";
                    using (var cmd = new MySqlCommand(roleQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", _projectId);
                        cmd.Parameters.AddWithValue("@userId", App.CurrentUser.Id);
                        var result = cmd.ExecuteScalar();
                        _userRole = result?.ToString() ?? "Пользователь";
                        System.Diagnostics.Debug.WriteLine($"Роль пользователя: {_userRole}, CanEdit: {CanEdit}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке роли пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _userRole = "Пользователь";
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке роли: {ex.Message}");
            }
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
                                System.Diagnostics.Debug.WriteLine($"Загружен пользователь: ID = {userId}, FullName = {user.FullName}");
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

                Responsible.ItemsSource = _responsiblePersons;
                Responsible.DisplayMemberPath = "FullName";
                Responsible.SelectedValuePath = "Id";

                if (_responsiblePersons.Count > 0)
                {
                    Responsible.SelectedIndex = 0;
                    System.Diagnostics.Debug.WriteLine($"Выбран первый пользователь: {_responsiblePersons[0].FullName}");
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка БД: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
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
                    System.Diagnostics.Debug.WriteLine($"Установлен ответственный: {responsible.FullName}");
                }
                else if (_responsiblePersons.Count > 0)
                {
                    Responsible.SelectedIndex = 0;
                    System.Diagnostics.Debug.WriteLine($"Ответственный не найден, выбранный первый: {_responsiblePersons[0].FullName}");
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
            if (!CanEdit)
            {
                MessageBox.Show("Только Создатель или Администратор могут создавать подзадачи.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(name.Text))
            {
                MessageBox.Show("Введите наименование подзадачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedUser = Responsible.SelectedItem as UserContext;
                if (selectedUser == null)
                {
                    MessageBox.Show("Выберите ответственного.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SubtaskContext subtask;
                if (_editingSubtask == null)
                {
                    subtask = new SubtaskContext
                    {
                        Name = name.Text,
                        Description = description.Text == "Опишите подробности" ? "" : description.Text,
                        DueDate = DateTime.Now.AddDays(7),
                        TaskId = _taskId,
                        UserId = selectedUser.Id,
                        StatusId = GetNewColumnId()
                    };
                    subtask.Add(); // Используем Add() вместо Save()
                    System.Diagnostics.Debug.WriteLine($"Новая подзадача добавлена: ID={subtask.Id}, Name={subtask.Name}");
                }
                else
                {
                    subtask = _editingSubtask;
                    subtask.Name = name.Text;
                    subtask.Description = description.Text == "Опишите подробности" ? "" : description.Text;
                    subtask.UserId = selectedUser.Id;
                    subtask.Update(); // Используем Update() вместо Save()
                    System.Diagnostics.Debug.WriteLine($"Подзадача обновлена: ID={subtask.Id}, Name={subtask.Name}");
                }

                CreatedSubtask?.Invoke(subtask);
                MessageBox.Show("Подзадача успешно добавлена/обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подготовке подзадачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
            return 1;
        }
    }
}