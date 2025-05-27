using Project.Classes;
using Project.Classes.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace Project.Pages
{
    public partial class EditProject : Page
    {
        private List<Participant> Participants { get; set; }
        private int ProjectId { get; set; }

        public EditProject(int projectId)
        {
            InitializeComponent();
            ProjectId = projectId;
            Participants = new List<Participant>();
            LoadProjectData();
            LoadParticipantsFromDatabase();
        }

        private void LoadProjectData()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    string query = "SELECT name, description, is_public FROM project WHERE id = @projectId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", ProjectId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtProjectName.Text = reader["name"].ToString();
                                txtProjectDescription.Text = reader["description"].ToString();
                                cmbPublicity.SelectedIndex = reader.GetBoolean("is_public") ? 0 : 1;
                            }
                        }
                    }

                    // Load participants
                    query = "SELECT u.id, u.fullName, pu.role FROM project_user pu JOIN user u ON pu.user = u.id WHERE pu.project = @projectId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", ProjectId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["role"].ToString() != "Создатель") // Creator is not editable
                                {
                                    Participants.Add(new Participant
                                    {
                                        Id = Convert.ToInt32(reader["id"]),
                                        Name = reader["fullName"].ToString(),
                                        Role = reader["role"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
                UpdateParticipantsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных проекта: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void LoadParticipantsFromDatabase()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    string query = "SELECT id, fullName FROM user WHERE id != @currentUserId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@currentUserId", App.CurrentUser.Id);
                        using (var reader = cmd.ExecuteReader())
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
                    MessageBox.Show("Вы уже являетесь создателем проекта.",
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

        private void Bt4_Save(object sender, RoutedEventArgs e)
        {
            string projectName = txtProjectName.Text;
            string projectDescription = txtProjectDescription.Text;
            bool isPublic = cmbPublicity.SelectedIndex == 0;

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

                // Update project details
                string updateQuery = "UPDATE project SET name = @name, description = @description, is_public = @is_public WHERE id = @projectId";
                using (var cmd = new MySqlCommand(updateQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@name", projectName);
                    cmd.Parameters.AddWithValue("@description", projectDescription);
                    cmd.Parameters.AddWithValue("@is_public", isPublic);
                    cmd.Parameters.AddWithValue("@projectId", ProjectId);
                    cmd.ExecuteNonQuery();
                }

                // Remove existing participants (except creator)
                string deleteParticipantsQuery = "DELETE FROM project_user WHERE project = @projectId AND role != 'Создатель'";
                using (var cmd = new MySqlCommand(deleteParticipantsQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@projectId", ProjectId);
                    cmd.ExecuteNonQuery();
                }

                // Add updated participants
                foreach (var participant in Participants)
                {
                    AddParticipantToProject(connection, transaction, ProjectId, participant.Id, participant.Role);
                }

                transaction.Commit();

                NavigationService?.Navigate(new Project1());
                MessageBox.Show("Проект успешно обновлен!",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                MessageBox.Show($"Ошибка при обновлении проекта: {ex.Message}",
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