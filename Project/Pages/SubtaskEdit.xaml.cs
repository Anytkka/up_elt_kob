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

        public int SubtaskNumber => _subtask?.Id ?? 0;
        public string SubtaskName { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool CanEdit => _userRole == "Создатель" || _userRole == "Администратор";
        public bool CanDelete => _userRole == "Создатель" || _userRole == "Администратор";

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
                if (_subtask == null) return;

                SubtaskName = _subtask.Name;
                Description = _subtask.Description;
                DueDate = _subtask.DueDate;

                var task = TaskContext.GetById(_subtask.TaskId);
                _projectId = int.Parse(task.ProjectCode);

                LoadUserRole();
                LoadResponsiblePersons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void LoadUserRole()
        {
            using (var connection = Connection.OpenConnection())
            {
                string query = @"SELECT role FROM project_user 
                                WHERE project = @projectId AND user = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@projectId", _projectId);
                    cmd.Parameters.AddWithValue("@userId", App.CurrentUser?.Id ?? 0);
                    _userRole = cmd.ExecuteScalar()?.ToString() ?? "Пользователь";
                }
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
                                cmbResponsible.Items.Add(new Participant
                                {
                                    Id = reader.GetInt32("id"),
                                    Name = reader.GetString("fullName")
                                });
                            }
                        }
                    }

                    // Текущий ответственный
                    var currentUser = _users.FirstOrDefault(u => u.Id == _subtask.UserId);
                    if (currentUser != null)
                    {
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
                MessageBox.Show($"Ошибка загрузки ответственных: {ex.Message}");
            }
        }

        private void BtSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEdit) return;

            try
            {
                _subtask.Name = SubtaskName;
                _subtask.Description = Description;
                _subtask.DueDate = DueDate;
                _subtask.UserId = _responsiblePersons.FirstOrDefault()?.Id ?? 0;

                _subtask.Update();

                MessageBox.Show("Изменения сохранены!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDelete) return;

            if (MessageBox.Show("Удалить подзадачу?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _subtask.Delete();
                    MessageBox.Show("Подзадача удалена!");
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        public class Participant
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private void cmbResponsible_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtAddResponsible_Click(object sender, RoutedEventArgs e)
        {
            if (cmbResponsible.SelectedItem is Participant selected)
            {
                _responsiblePersons.Clear(); // Для подзадачи только один ответственный
                _responsiblePersons.Add(selected);
                listViewResponsible.Items.Refresh();
            }
        }
    }
}
           