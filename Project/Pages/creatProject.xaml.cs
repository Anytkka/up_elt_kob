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
    public partial class creatProject : Page
    {
        private List<Participant> Participants { get; set; }

        public creatProject()
        {
            InitializeComponent();
            Participants = new List<Participant>();
            LoadParticipantsFromDatabase();
        }

        private void LoadParticipantsFromDatabase()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    string query = "SELECT id, fullName FROM user";
                    using (var reader = Connection.Query(query, connection))
                    {
                        cmbParticipants.Items.Clear();
                        while (reader.Read())
                        {
                            var item = new ComboBoxItem
                            {
                                Content = reader["fullName"].ToString(),
                                Tag = Convert.ToInt32(reader["id"])
                            };
                            cmbParticipants.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке участников: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void Bt_AddUsers(object sender, RoutedEventArgs e)
        {
            if (cmbParticipants.SelectedItem is ComboBoxItem selectedItem)
            {
                int participantId = (int)selectedItem.Tag;
                string participantName = selectedItem.Content.ToString();

                if (participantId == App.CurrentUser.Id)
                {
                    MessageBox.Show("Вы автоматически будете добавлены как Создатель проекта.",
                                  "Информация",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    return;
                }

                string role = rbAdmin.IsChecked == true ? "Администратор" : "Пользователь";

                if (!Participants.Any(p => p.Id == participantId))
                {
                    Participants.Add(new Participant
                    {
                        Id = participantId,
                        Name = participantName,
                        Role = role
                    });
                    UpdateParticipantsList();
                }
                else
                {
                    MessageBox.Show("Этот участник уже добавлен в проект",
                                  "Предупреждение",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
            }
        }

        private void UpdateParticipantsList()
        {
            listViewParticipants.ItemsSource = null;
            listViewParticipants.ItemsSource = Participants;
        }

        private void RemoveParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Participant participant)
            {
                Participants.Remove(participant);
                UpdateParticipantsList();
            }
        }

        private void Bt4_Cancel(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Bt4_Create(object sender, RoutedEventArgs e)
        {
            string projectName = txtProjectName.Text;
            string projectDescription = txtProjectDescription.Text;
            bool is_public = cmbPublicity.SelectedIndex == 0;

            if (string.IsNullOrWhiteSpace(projectName))
            {
                MessageBox.Show("Пожалуйста, введите наименование проекта.",
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
                    // Создаем проект
                    string insertQuery = "INSERT INTO project (name, description, is_public) VALUES (@name, @description, @is_public)";
                    using (var cmd = new MySqlCommand(insertQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@name", projectName);
                        cmd.Parameters.AddWithValue("@description", projectDescription);
                        cmd.Parameters.AddWithValue("@is_public", is_public);
                        cmd.ExecuteNonQuery();
                    }

                    // Получаем ID только что созданного проекта
                    int projectId;
                    using (var cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", connection, transaction))
                    {
                        projectId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Создаем стандартные столбцы для канбан-доски
                    string[] defaultColumns = { "Новые", "В процессе", "Можно проверять", "Готово" };
                    foreach (var columnTitle in defaultColumns)
                    {
                        string insertColumnQuery = "INSERT INTO kanbanColumn (title_status, project) VALUES (@title_status, @projectId)";
                        using (var cmd = new MySqlCommand(insertColumnQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@title_status", columnTitle);
                            cmd.Parameters.AddWithValue("@projectId", projectId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Добавляем текущего пользователя как Создателя
                    AddParticipantToProject(connection, transaction, projectId, App.CurrentUser.Id, "Создатель");

                    // Добавляем остальных участников к проекту
                    foreach (var participant in Participants)
                    {
                        AddParticipantToProject(connection, transaction, projectId, participant.Id, participant.Role);
                    }

                    transaction.Commit();

                    NavigationService?.Navigate(new Project1());
                    MessageBox.Show("Проект успешно создан!",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show($"Ошибка при создании проекта: {ex.Message}",
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

        private void AddParticipantToProject(MySqlConnection connection, MySqlTransaction transaction, int projectId, int userId, string role)
        {
            try
            {
                string query = "INSERT INTO project_user (project, user, role) VALUES (@projectId, @userId, @role)";
                using (var command = new MySqlCommand(query, connection, transaction))
                {
                    command.Parameters.AddWithValue("@projectId", projectId);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@role", role);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при добавлении участника: {ex.Message}");
            }
        }

        public class Participant
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
        }
    }
}