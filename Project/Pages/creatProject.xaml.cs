using Project.Classes;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    public partial class creatProject : Page
    {
        private List<string> Participants { get; set; }

        public creatProject()
        {
            InitializeComponent();
            Participants = new List<string>();
        }

        //добавление участника
        private void Bt_AddUsers_Click(object sender, RoutedEventArgs e)
        {

            string participantName = "Новый Участник";
            Participants.Add(participantName);


            UpdateParticipantsList();
        }

        private void UpdateParticipantsList()
        {
            listViewParticipants.Items.Clear();
            foreach (var participant in Participants)
            {
                var item = new ListViewItem();
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var label = new Label { Content = participant };
                var button = new Button
                {
                    Content = "✕",
                    Foreground = System.Windows.Media.Brushes.Red,
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    FontWeight = FontWeights.Bold,
                    Tag = participant
                };
                button.Click += RemoveParticipant_Click;

                grid.Children.Add(label);
                grid.Children.Add(button);
                Grid.SetColumn(button, 1);

                item.Content = grid;
                listViewParticipants.Items.Add(item);
            }
        }

        private void RemoveParticipant_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string participantName = button.Tag.ToString();
                Participants.Remove(participantName);
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

                MessageBox.Show("Проект успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

