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
            MySqlConnection connection = null;
            MySqlDataReader reader = null;

            try
            {
                connection = Connection.OpenConnection();
                string query = "SELECT name FROM user";
                reader = Connection.Query(query, connection);

                cmbParticipants.Items.Clear(); // Очищаем список перед загрузкой

                while (reader.Read())
                {
                    string participantName = reader["name"].ToString();
                    cmbParticipants.Items.Add(participantName); // Упрощенное добавление
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке участников: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                reader?.Close();
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    Connection.CloseConnection(connection);
                }
            }
        }

        private void Bt_AddUsers(object sender, RoutedEventArgs e)
        {
            if (cmbParticipants.SelectedItem != null)
            {
                string participantName = cmbParticipants.SelectedItem.ToString();
                string role = rbAdmin.IsChecked == true ? "Администратор" : "Пользователь";

                if (!Participants.Any(p => p.Name == participantName))
                {
                    Participants.Add(new Participant { Name = participantName, Role = role });
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
            bool isPublic = cmbPublicity.SelectedIndex == 0;

            if (string.IsNullOrWhiteSpace(projectName))
            {
                MessageBox.Show("Пожалуйста, введите наименование проекта.",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                var project = new ProjectContext(0, projectName, projectDescription, isPublic);
                project.Add();

                // Добавляем участников к проекту
                foreach (var participant in Participants)
                {
                    AddParticipantToProject(project.Id, participant.Name, participant.Role);
                }

                NavigationService?.Navigate(new Project1(project));
                MessageBox.Show("Проект успешно создан!",
                              "Успех",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании проекта: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void AddParticipantToProject(int projectId, string participantName, string role)
        {
            MySqlConnection connection = null;

            try
            {
                connection = Connection.OpenConnection();
                string query = "INSERT INTO project_participants (project_id, user_name, role) VALUES (@projectId, @userName, @role)";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@projectId", projectId);
                    command.Parameters.AddWithValue("@userName", participantName);
                    command.Parameters.AddWithValue("@role", role);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении участника в проект: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            finally
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    Connection.CloseConnection(connection);
                }
            }
        }
    }

    public class Participant
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
