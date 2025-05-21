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
            // Здесь можно реализовать выбор ответственных из списка,
            // аналогично тому как это сделано в creatProject
            // или перейти на страницу выбора ответственных
        }

        private void UpdateResponsibleList()
        {
            // Обновляем список ответственных в ListView
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
            NavigationService?.Navigate(new AddSubtask(0)); // Здесь 0 - временный taskId
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
            // Обновляем список подзадач в ListView
        }

        private void Bt5_Cancel(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Bt5_Create(object sender, RoutedEventArgs e)
        {
            string taskName = txtTaskName.Text;
            string taskDescription = txtTaskDescription.Text;

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

            try
            {
                connection = Connection.OpenConnection();
                transaction = connection.BeginTransaction();

                try
                {
                    // Создаем задачу
                    string insertQuery = "INSERT INTO task (name, description) VALUES (@name, @description)";
                    using (var cmd = new MySqlCommand(insertQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@name", taskName);
                        cmd.Parameters.AddWithValue("@description", taskDescription);
                        cmd.ExecuteNonQuery();
                    }

                    // Получаем ID только что созданной задачи
                    int taskId;
                    using (var cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", connection, transaction))
                    {
                        taskId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Добавляем ответственных к задаче
                    foreach (var participant in ResponsiblePersons)
                    {
                        AddResponsibleToTask(connection, transaction, taskId, participant.Id);
                    }

                    // Добавляем подзадачи
                    foreach (var subtask in Subtasks)
                    {
                        subtask.TaskId = taskId;
                        subtask.Add(connection);
                    }

                    transaction.Commit();

                    MessageBox.Show("Задача успешно создана!",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                   
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