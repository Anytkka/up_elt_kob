using Project.Classes;
using Project.Classes.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using Project.Main;

namespace Project.Pages
{
    public partial class TaskEdit : Page
    {
        private int _taskId;
        private int _projectId;
        private string _userRole;
        private List<Participant> ResponsiblePersons { get; set; }
        private List<SubtaskContext> Subtasks { get; set; }

        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public DateTime DueDate { get; set; }
        public bool CanEdit => _userRole == "Создатель" || _userRole == "Администратор";
        public bool IsReadOnly => !CanEdit;

        public TaskEdit(int taskId)
        {
            InitializeComponent();
            _taskId = taskId;
            ResponsiblePersons = new List<Participant>();
            Subtasks = new List<SubtaskContext>();
            LoadTaskDetails(); // Сначала загружаем детали задачи, чтобы определить _projectId
            LoadUserRole(); // Затем загружаем роль пользователя
            LoadResponsiblePersonsFromDatabase();
            DataContext = this;
        }

        private void LoadUserRole()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    // Используем уже загруженный _projectId
                    if (_projectId > 0)
                    {
                        string roleQuery = "SELECT role FROM project_user WHERE project = @projectId AND user = @userId";
                        using (var cmd = new MySqlCommand(roleQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@projectId", _projectId);
                            cmd.Parameters.AddWithValue("@userId", App.CurrentUser?.Id ?? 0);
                            var result = cmd.ExecuteScalar();
                            _userRole = result?.ToString() ?? "Пользователь";
                        }
                    }
                    else
                    {
                        _userRole = "Пользователь"; // По умолчанию, если проект не найден
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке роли пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _userRole = "Пользователь"; // По умолчанию
            }
        }

        private void LoadTaskDetails()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    // Получаем projectId через kanbanColumn
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
                            _projectId = Convert.ToInt32(result);
                        }
                    }

                    // Загружаем детали задачи
                    string taskQuery = "SELECT name, description, dueDate FROM task WHERE id = @taskId";
                    using (var cmd = new MySqlCommand(taskQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@taskId", _taskId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TaskName = reader["name"].ToString();
                                TaskDescription = reader["description"].ToString();
                                DueDate = reader.GetDateTime("dueDate");
                                txtTaskName.Text = TaskName;
                                txtTaskDescription.Text = TaskDescription;
                                dpDueDate.SelectedDate = DueDate;
                            }
                        }
                    }

                    // Загружаем ответственных
                    string responsibleQuery = @"SELECT u.id, u.fullName AS Name
                                             FROM task_user tu
                                             JOIN user u ON tu.user = u.id
                                             WHERE tu.task = @taskId";
                    using (var cmd = new MySqlCommand(responsibleQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@taskId", _taskId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ResponsiblePersons.Add(new Participant
                                {
                                    Id = reader.GetInt32("id"),
                                    Name = reader["Name"].ToString()
                                });
                            }
                            UpdateResponsibleList();
                        }
                    }

                    // Загружаем подзадачи
                    string subtaskQuery = "SELECT id, name, description, dueDate, task, user, status FROM subtask WHERE task = @taskId";
                    using (var cmd = new MySqlCommand(subtaskQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@taskId", _taskId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Subtasks.Add(new SubtaskContext(
                                    reader.GetInt32("id"),
                                    reader.GetString("name"),
                                    reader.GetString("description"),
                                    reader.GetDateTime("dueDate"),
                                    reader.GetInt32("task"),
                                    reader.GetInt32("user"),
                                    reader.GetInt32("status")
                                ));
                            }
                            UpdateSubtasksList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных задачи: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void LoadResponsiblePersonsFromDatabase()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    string query = @"SELECT DISTINCT u.id, u.fullName
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
                                    Id = Convert.ToInt32(reader["id"]),
                                    Name = reader["fullName"].ToString()
                                };
                                cmbResponsible.Items.Add(participant);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке ответственных: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void BtAddResponsible_Click(object sender, RoutedEventArgs e)
        {
            if (cmbResponsible.SelectedItem is Participant selectedParticipant)
            {
                if (!ResponsiblePersons.Any(p => p.Id == selectedParticipant.Id))
                {
                    ResponsiblePersons.Add(selectedParticipant);
                    UpdateResponsibleList();
                }
            }
        }

        private void UpdateResponsibleList()
        {
            listViewResponsible.ItemsSource = null;
            listViewResponsible.ItemsSource = ResponsiblePersons;
        }

        private void RemoveResponsible_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Participant participant)
            {
                ResponsiblePersons.Remove(participant);
                UpdateResponsibleList();
            }
        }

        private void BtAddSubtask_Click(object sender, RoutedEventArgs e)
        {
            var addSubtaskPage = new AddSubtask(_taskId, null, _projectId);
            addSubtaskPage.CreatedSubtask += OnSubtaskCreated;
            NavigationService?.Navigate(addSubtaskPage);
        }

        private void OnSubtaskCreated(SubtaskContext subtask)
        {
            if (subtask != null)
            {
                Subtasks.Add(new SubtaskContext(
                    0, // Временный ID, будет обновлен при сохранении
                    subtask.Name,
                    subtask.Description ?? "",
                    subtask.DueDate,
                    _taskId,
                    subtask.UserId,
                    subtask.StatusId
                ));
                UpdateSubtasksList();
            }
        }

        private void UpdateSubtasksList()
        {
            listViewSubtasks.ItemsSource = null;
            listViewSubtasks.ItemsSource = Subtasks.Select(s => new SubtaskCard
            {
                SubtaskNumber = s.Id,
                SubtaskName = s.Name,
                Responsible = UserContext.Get().FirstOrDefault(u => u.Id == s.UserId)?.FullName ?? "Не указан",
                TaskCode = _taskId.ToString(),
                TaskName = TaskName,
                UserRole = _userRole // Передаем роль пользователя
            }).ToList();

            // Подписываемся на события для каждого SubtaskCard
            foreach (SubtaskCard card in listViewSubtasks.Items)
            {
                card.EditButtonClicked += SubtaskCard_EditButtonClicked;
                card.DeleteButtonClicked += SubtaskCard_DeleteButtonClicked;
            }
        }

        private void SubtaskCard_EditButtonClicked(object sender, int subtaskId)
        {
            System.Diagnostics.Debug.WriteLine($"Navigating to SubtaskEdit for Subtask ID: {subtaskId}");
            NavigationService?.Navigate(new SubtaskEdit(subtaskId));
        }

        private void SubtaskCard_DeleteButtonClicked(object sender, int subtaskId)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить эту подзадачу?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var subtaskToRemove = Subtasks.FirstOrDefault(s => s.Id == subtaskId);
                    if (subtaskToRemove != null)
                    {
                        subtaskToRemove.Delete();
                        Subtasks.Remove(subtaskToRemove);
                        UpdateSubtasksList();
                        MessageBox.Show("Подзадача удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void BtSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEdit)
            {
                MessageBox.Show("Только Создатель и Администратор могут редактировать задачи.",
                              "Ошибка доступа",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            TaskName = txtTaskName.Text;
            TaskDescription = txtTaskDescription.Text;
            DueDate = dpDueDate.SelectedDate ?? DateTime.Now.AddDays(7);

            if (string.IsNullOrWhiteSpace(TaskName))
            {
                MessageBox.Show("Пожалуйста, введите наименование задачи.",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            MySqlConnection connection = null;
            MySqlTransaction transaction = null;

            try
            {
                connection = Connection.OpenConnection();
                transaction = connection.BeginTransaction();

                try
                {
                    int statusId = GetCurrentStatusId(connection);

                    string updateQuery = @"UPDATE task
                                        SET name = @name,
                                            description = @description,
                                            dueDate = @dueDate,
                                            status = @statusId
                                        WHERE id = @taskId";

                    using (var cmd = new MySqlCommand(updateQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@name", TaskName);
                        cmd.Parameters.AddWithValue("@description", TaskDescription);
                        cmd.Parameters.AddWithValue("@dueDate", DueDate);
                        cmd.Parameters.AddWithValue("@statusId", statusId);
                        cmd.Parameters.AddWithValue("@taskId", _taskId);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteResponsibleQuery = "DELETE FROM task_user WHERE task = @taskId";
                    using (var cmd = new MySqlCommand(deleteResponsibleQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@taskId", _taskId);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (var participant in ResponsiblePersons)
                    {
                        string responsibleQuery = @"INSERT INTO task_user (task, user)
                                                 VALUES (@taskId, @userId)";
                        using (var cmd = new MySqlCommand(responsibleQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@taskId", _taskId);
                            cmd.Parameters.AddWithValue("@userId", participant.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    string deleteSubtasksQuery = "DELETE FROM subtask WHERE task = @taskId";
                    using (var cmd = new MySqlCommand(deleteSubtasksQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@taskId", _taskId);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (var subtask in Subtasks)
                    {
                        subtask.TaskId = _taskId;
                        string subtaskQuery = @"INSERT INTO subtask
                                              (task, name, description, dueDate, user, status)
                                              VALUES (@task, @name, @description, @dueDate, @user, @status)";
                        using (var cmd = new MySqlCommand(subtaskQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@task", subtask.TaskId);
                            cmd.Parameters.AddWithValue("@name", subtask.Name);
                            cmd.Parameters.AddWithValue("@description", subtask.Description ?? "");
                            cmd.Parameters.AddWithValue("@dueDate", subtask.DueDate);
                            cmd.Parameters.AddWithValue("@user", subtask.UserId);
                            cmd.Parameters.AddWithValue("@status", subtask.StatusId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();

                    MessageBox.Show("Задача успешно обновлена!",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    NavigationService?.Navigate(new Kanban(_projectId));
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show($"Ошибка при обновлении задачи: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                connection?.Close();
            }
        }

        private int GetCurrentStatusId(MySqlConnection connection)
        {
            string query = "SELECT status FROM task WHERE id = @taskId";
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@taskId", _taskId);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 1;
            }
        }

        public class Participant
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}