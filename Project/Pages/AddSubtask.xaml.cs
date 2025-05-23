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
                    MessageBox.Show("ID проекта не указан", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _responsiblePersons = new List<UserContext>();
                using (var connection = Connection.OpenConnection())
                {
                    if (connection == null)
                    {
                        MessageBox.Show("Не удалось подключиться к БД", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string query = @"SELECT u.id, u.fullName
                           FROM user u
                           JOIN project_user pu ON u.id = pu.user_id
                           WHERE pu.project_id = @projectId";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", _projectId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                _responsiblePersons.Add(new UserContext(
                                    Convert.ToInt32(reader["id"]),
                                    reader["fullName"].ToString(),
                                    "", "", ""
                                ));
                            }
                        }
                    }
                }

                Responsible.ItemsSource = _responsiblePersons;

                if (_responsiblePersons.Count == 0)
                {
                    MessageBox.Show("В проекте нет пользователей. Проверьте БД.",
                                  "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    Responsible.SelectedIndex = 0;
                }
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
                    Responsible.SelectedItem = responsible;
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
                MessageBox.Show("Введите наименование подзадачи", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var selectedUser = Responsible.SelectedItem as UserContext;
                if (selectedUser == null)
                {
                    MessageBox.Show("Выберите ответственного", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_editingSubtask == null)
                {
                    var subtask = new SubtaskContext(
                        0,
                        name.Text,
                        description.Text == "Опишите подробности" ? "" : description.Text,
                        DateTime.Now.AddDays(7),
                        _taskId,
                        selectedUser.Id,
                        1 // Статус по умолчанию
                    );

                    CreatedSubtask?.Invoke(subtask);
                }
                else
                {
                    _editingSubtask.Name = name.Text;
                    _editingSubtask.Description = description.Text == "Опишите подробности" ? "" : description.Text;
                    _editingSubtask.UserId = selectedUser.Id;

                    CreatedSubtask?.Invoke(_editingSubtask);
                }

                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подготовке подзадачи: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
