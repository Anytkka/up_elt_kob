using Project.Classes;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    public partial class Project1 : Page
    {
        private List<ProjectContext> Projects { get; set; }

        public Project1(ProjectContext newProject)
        {
            InitializeComponent();
            Projects = new List<ProjectContext> { newProject };
            DisplayProjects();
        }

        private void DisplayProjects()
        {
            ProjectsList.ItemsSource = Projects;
        }

        private void Bt_CreateProject(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new creatProject());
        }

        private void Bt1_Projects(object sender, RoutedEventArgs e)
        {
            // Логика для отображения списка проектов
            ProjectsGrid.Visibility = Visibility.Visible;
            NoProjectsGrid.Visibility = Visibility.Collapsed;
        }

        private void PAText_MouseDown(object sender, RoutedEventArgs e)
        {
            // Логика для обработки нажатия на текст профиля
        }
    }
}
