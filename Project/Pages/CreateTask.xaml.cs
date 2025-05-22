using Project.Classes;
using Project.Classes.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Linq;

namespace Project.Pages
{
    public partial class CreateTask : Page
    {
        public event Action<int> TaskCreated; // Добавляем событие

        private List<Participant> ResponsiblePersons { get; set; }
        private List<SubtaskContext> Subtasks { get; set; }

        public CreateTask()
        {
            InitializeComponent();
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
                    string query = "SELECT id, fullName FROM user";
                    using (var reader = Connection.Query(query, connection))
                    {
                        while (reader.Read())
                        {
                            var participant = new Participant
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["fullName"].ToString()
                            };
                            ResponsiblePersons.Add(participant);
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
            // Реализация добавления ответственных
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
            // Переход на страницу добавления подзадачи
            NavigationService?.Navigate(new AddSubtask(0));
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

        private void Bt5_Create(object sender, RoutedEventArgs e)
        {
            string taskName = txtTaskName.Text;
            string taskDescription = txtTaskDescription.Text;
            DateTime dueDate = DateTime.Now.AddDays(7); // Установите реальную дату из вашего UI

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

            try
            {
                connection = Connection.OpenConnection();
                transaction = connection.BeginTransaction();

                try
                {
                    // Создаем задачу (без projectId)
                    string insertQuery = @"INSERT INTO task 
                                (name, description, dueDate, status) 
                                VALUES (@name, @description, @dueDate, @status);
                                SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(insertQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@name", taskName);
                        cmd.Parameters.AddWithValue("@description", taskDescription);
                        cmd.Parameters.AddWithValue("@dueDate", dueDate);
                        cmd.Parameters.AddWithValue("@status", 0); // Статус по умолчанию (например, "Новая")

                        taskId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Добавляем ответственных к задаче
                    foreach (var participant in ResponsiblePersons)
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

                    // Добавляем подзадачи
                    foreach (var subtask in Subtasks)
                    {
                        string subtaskQuery = @"INSERT INTO subtask 
                                      (task_id, name, description) 
                                      VALUES (@taskId, @name, @description)";

                        using (var cmd = new MySqlCommand(subtaskQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@taskId", taskId);
                            cmd.Parameters.AddWithValue("@name", subtask.Name);
                            cmd.Parameters.AddWithValue("@description", subtask.Description);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();

                    MessageBox.Show("Задача успешно создана!",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    // Возвращаемся на предыдущую страницу
                    NavigationService?.GoBack();
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

        private void AddResponsibleToTask(MySqlConnection connection, MySqlTransaction transaction, int taskId, int userId)
        {
            try
            {
                string query = "INSERT INTO task_user (task, user) VALUES (@taskId, @userId)";

                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@taskId", taskId);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при добавлении ответственного: {ex.Message}");
            }
        }

        public class Participant
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

    }
}