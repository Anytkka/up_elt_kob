﻿using Project.Classes;
using Project.Classes.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MySql.Data.MySqlClient;

namespace Project.Pages
{
    public partial class CreateTask : Page
    {
        public event Action<int> TaskCreated;
        private List<Participant> ResponsiblePersons { get; set; }
        private List<SubtaskContext> Subtasks { get; set; }
        private int _projectId;

        public CreateTask(int projectId)
        {
            InitializeComponent();
            _projectId = projectId;
            ResponsiblePersons = new List<Participant>();
            Subtasks = new List<SubtaskContext>();
            LoadResponsiblePersonsFromDatabase();
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

        private void Bt5_AddUsers(object sender, RoutedEventArgs e)
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

        private void Bt5_AddS(object sender, RoutedEventArgs e)
        {
            var addSubtaskPage = new AddSubtask(0, null, _projectId);
            addSubtaskPage.CreatedSubtask += OnSubtaskCreated;
            NavigationService?.Navigate(addSubtaskPage);
        }

        private void OnSubtaskCreated(SubtaskContext subtask)
        {
            if (subtask != null) // Исправлено с thickened на null
            {
                Subtasks.Add(subtask);
                UpdateSubtasksList();
            }
        }

        private void Bt5_DeleteS(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is SubtaskContext subtask)
            {
                Subtasks.Remove(subtask);
                UpdateSubtasksList();
            }
        }

        private void UpdateSubtasksList()
        {
            listViewSubtasks.ItemsSource = null;
            listViewSubtasks.ItemsSource = Subtasks;
        }

        private void Bt5_Cancel(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void RefreshTaskDisplay(int taskId)
        {
            Console.WriteLine($"Task with ID {taskId} created at {DateTime.Now}, refreshing display...");
            TaskCreated?.Invoke(taskId);
        }

        private void Bt5_Create(object sender, RoutedEventArgs e)
        {
            string taskName = txtTaskName.Text;
            string taskDescription = txtTaskDescription.Text;
            DateTime dueDate = DateTime.Now.AddDays(7);

            if (string.IsNullOrWhiteSpace(taskName))
            {
                MessageBox.Show("Пожалуйста, введите наименование задачи.",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            MySqlConnection connection = null;
            MySqlTransaction transaction = null;
            int taskId = 0;
            int statusId = 0;

            try
            {
                connection = Connection.OpenConnection();
                transaction = connection.BeginTransaction();

                try
                {
                    string statusQuery = @"SELECT id FROM kanbanColumn
                         WHERE project = @projectId AND title_status = 'Новые'";
                    using (var cmd = new MySqlCommand(statusQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@projectId", _projectId);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            statusId = Convert.ToInt32(result);
                        }
                        else
                        {
                            string insertStatusQuery = @"INSERT INTO kanbanColumn (title_status, project)
                                       VALUES ('Новые', @projectId);
                                       SELECT LAST_INSERT_ID();";
                            using (var insertCmd = new MySqlCommand(insertStatusQuery, connection, transaction))
                            {
                                insertCmd.Parameters.AddWithValue("@projectId", _projectId);
                                statusId = Convert.ToInt32(insertCmd.ExecuteScalar());
                            }
                        }
                    }

                    if (!StatusExists(connection, statusId))
                    {
                        throw new Exception($"Статус с ID {statusId} не существует в базе данных.");
                    }

                    string insertQuery = @"INSERT INTO task
                (name, description, dueDate, status)
                VALUES (@name, @description, @dueDate, @statusId);
                SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(insertQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@name", taskName);
                        cmd.Parameters.AddWithValue("@description", taskDescription);
                        cmd.Parameters.AddWithValue("@dueDate", dueDate);
                        cmd.Parameters.AddWithValue("@statusId", statusId);
                        taskId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    foreach (var participant in ResponsiblePersons)
                    {
                        if (UserExists(connection, participant.Id))
                        {
                            string responsibleQuery = @"INSERT INTO task_user
                                     (task, user)
                                     VALUES (@taskId, @userId)";

                            using (var cmd = new MySqlCommand(responsibleQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@taskId", taskId);
                                cmd.Parameters.AddWithValue("@userId", participant.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            throw new Exception($"Участник с ID {participant.Id} не существует в базе данных.");
                        }
                    }

                    // Сохранение подзадач с обновленным taskId
                    foreach (var subtask in Subtasks)
                    {
                        subtask.TaskId = taskId; // Обновляем TaskId подзадачи
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
                            cmd.Parameters.AddWithValue("@status", statusId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();

                    MessageBox.Show("Задача успешно создана!",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    RefreshTaskDisplay(taskId);
                    NavigationService?.Navigate(new Kanban(_projectId));
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show($"Ошибка при создании задачи: {ex.Message}",
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

        private bool StatusExists(MySqlConnection connection, int statusId)
        {
            string query = "SELECT COUNT(*) FROM kanbanColumn WHERE id = @statusId";
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@statusId", statusId);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private bool UserExists(MySqlConnection connection, int userId)
        {
            string query = "SELECT COUNT(*) FROM user WHERE id = @userId";
            using (var cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public class Participant
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}