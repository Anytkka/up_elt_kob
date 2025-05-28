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
    public partial class SubtaskEdit : Page
    {
        private int _subtaskId;
        private int _projectId;
        private string _userRole;
        private List<Participant> _responsiblePersons = new List<Participant>();
        private SubtaskContext _subtask;
        private List<UserContext> _users = new List<UserContext>();

        public event EventHandler<int> SubtaskUpdated;

        public int SubtaskNumber => _subtask?.Id ?? 0;
        public string SubtaskName { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool CanEdit => _userRole == "Создатель" || _userRole == "Администратор";
        public bool CanDelete => _userRole == "Создатель" || _userRole == "Администратор";
        public bool IsReadOnly => !CanEdit;

        public SubtaskEdit(int subtaskId)
        {
            InitializeComponent();
            _subtaskId = subtaskId;
            LoadSubtaskData();
            DataContext = this;
        }

        private void LoadSubtaskData()
        {
            try
            {
                _subtask = SubtaskContext.GetById(_subtaskId);
                if (_subtask == null)
                {
                    MessageBox.Show("Подзадача не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.GoBack();
                    return;
                }

                SubtaskName = _subtask.Name;
                Description = _subtask.Description;
                DueDate = _subtask.DueDate;

                var task = TaskContext.GetById(_subtask.TaskId);
                if (task != null && int.TryParse(task.ProjectCode, out int projectId))
                {
                    _projectId = projectId;
                }

                LoadUserRole();
                LoadResponsiblePersons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserRole()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    string query = @"SELECT role FROM project_user 
                                    WHERE project = @projectId AND user = @userId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", _projectId);
                        cmd.Parameters.AddWithValue("@userId", App.CurrentUser?.Id ?? 0);
                        var result = cmd.ExecuteScalar();
                        _userRole = result?.ToString() ?? "Пользователь";
                        System.Diagnostics.Debug.WriteLine($"User role loaded: {_userRole}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки роли: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _userRole = "Пользователь";
            }
        }

        private void LoadResponsiblePersons()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    string query = @"SELECT u.id, u.fullName 
                                  FROM user u
                                  INNER JOIN project_user pu ON u.id = pu.user
                                  WHERE pu.project = @projectId";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", _projectId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            cmbResponsible.Items.Clear();
                            while (reader.Read())
                            {
                                var participant = new Participant
                                {
                                    Id = reader.GetInt32("id"),
                                    Name = reader.GetString("fullName")
                                };
                                cmbResponsible.Items.Add(participant);
                            }
                        }
                    }

                    // Текущий ответственный
                    var currentUser = UserContext.Get().FirstOrDefault(u => u.Id == _subtask.UserId);
                    if (currentUser != null)
                    {
                        _responsiblePersons.Clear();
                        _responsiblePersons.Add(new Participant
                        {
                            Id = currentUser.Id,
                            Name = currentUser.FullName
                        });
                        listViewResponsible.ItemsSource = _responsiblePersons;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ответственных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEdit)
            {
                MessageBox.Show("Только Создатель или Администратор могут редактировать подзадачу.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSubtaskName.Text))
            {
                MessageBox.Show("Введите наименование подзадачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _subtask.Name = txtSubtaskName.Text;
                _subtask.Description = txtSubtaskDescription.Text;
                _subtask.DueDate = dpDueDate.SelectedDate ?? DateTime.Now.AddDays(7);
                _subtask.UserId = _responsiblePersons.FirstOrDefault()?.Id ?? 0;

                _subtask.Update();

                SubtaskUpdated?.Invoke(this, _subtaskId);

                MessageBox.Show("Изменения сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void BtAddResponsible_Click(object sender, RoutedEventArgs e)
        {
            if (cmbResponsible.SelectedItem is Participant selected)
            {
                _responsiblePersons.Clear(); // Для подзадачи только один ответственный
                _responsiblePersons.Add(selected);
                listViewResponsible.ItemsSource = _responsiblePersons;
                listViewResponsible.Items.Refresh();
                System.Diagnostics.Debug.WriteLine($"Responsible user set: {selected.Name}");
            }
        }

        public class Participant
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}