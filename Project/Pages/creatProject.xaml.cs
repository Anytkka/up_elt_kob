using Project.Classes;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

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
            string connectionString = "Server=your_server;Database=your_db;Uid=your_username;Pwd=your_password;";
            string query = "SELECT name FROM users";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string participantName = reader["name"].ToString();
                    cmbParticipants.Items.Add(new ComboBoxItem { Content = participantName });
                }
            }
        }

        private void Bt_AddUsers_Click(object sender, RoutedEventArgs e)
        {
            if (cmbParticipants.SelectedItem != null)
            {
                string participantName = (cmbParticipants.SelectedItem as ComboBoxItem).Content.ToString();
                string role = rbAdmin.IsChecked == true ? "Администратор" : "Пользователь";

                Participants.Add(new Participant { Name = participantName, Role = role });

                UpdateParticipantsList();
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

        private void Bt4_Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Bt4_Create_Click(object sender, RoutedEventArgs e)
        {
            string projectName = txtProjectName.Text;
            string projectDescription = txtProjectDescription.Text;
            bool isPublic = cmbPublicity.SelectedIndex == 0;

            if (string.IsNullOrEmpty(projectName))
            {
                MessageBox.Show("Пожалуйста, введите наименование проекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var project = new ProjectContext(0, projectName, projectDescription, isPublic);

            try
            {
                project.Add();

                // Переход на страницу с проектами и отображение созданного проекта
                NavigationService?.Navigate(new Project1(project));

                MessageBox.Show("Проект успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Participant
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
