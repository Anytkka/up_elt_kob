using Project.Classes;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using Project.Classes.Common;

namespace Project.Pages
{
    public partial class ProjectDetails : Page
    {
        private int ProjectId { get; set; }

        public ProjectDetails(int projectId)
        {
            InitializeComponent();
            ProjectId = projectId;
            LoadProjectDetails();
        }

        private void LoadProjectDetails()
        {
            try
            {
                using (var connection = Connection.OpenConnection())
                {
                    string projectQuery = "SELECT name, description, is_public FROM project WHERE id = @projectId";
                    using (var cmd = new MySqlCommand(projectQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", ProjectId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtProjectName.Text = reader["name"].ToString();
                                txtProjectDescription.Text = reader["description"].ToString();
                                txtPublicity.Text = reader.GetBoolean("is_public") ? "Открытый" : "Закрытый";
                            }
                        }
                    }

                    // Загружаем участников проекта
                    string participantsQuery = "SELECT u.fullName, pu.role FROM project_user pu JOIN user u ON pu.user = u.id WHERE pu.project = @projectId";
                    using (var cmd = new MySqlCommand(participantsQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@projectId", ProjectId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            var participants = new List<Participant>();
                            while (reader.Read())
                            {
                                participants.Add(new Participant
                                {
                                    Name = reader["fullName"].ToString(),
                                    Role = reader["role"].ToString()
                                });
                            }
                            listViewParticipants.ItemsSource = participants;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных проекта: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        public class Participant
        {
            public string Name { get; set; }
            public string Role { get; set; }
        }
    }
}