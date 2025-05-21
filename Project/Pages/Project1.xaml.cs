using Project.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    public partial class Project1 : Page
    {
        public Project1()
        {
            InitializeComponent();
            this.DataContext = App.CurrentUser;
            LoadProjects();
        }

        private void LoadProjects()
        {
            ProjectsPanel.Children.Clear();

            var currentUser = App.CurrentUser;
            if (currentUser == null) return;

            var userProjects = ProjUserContext.GetByUserId(currentUser.Id)
                .Where(pu => pu.Role == "Создатель" || pu.Role == "Администратор" || pu.Role == "Пользователь")
                .ToList();

            if (userProjects.Count == 0)
            {
                var noProjectsText = new TextBlock
                {
                    Text = "У вас пока нет проектов",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16
                };
                ProjectsPanel.Children.Add(noProjectsText);
                return;
            }
            foreach (var projUser in userProjects)
            {
                var project = ProjectContext.Get().FirstOrDefault(p => p.Id == projUser.Project);
                if (project == null) continue;

                var creatorRelation = ProjUserContext.GetByProjectId(project.Id)
                    .FirstOrDefault(pu => pu.Role == "Создатель");

                if (creatorRelation == null) continue;

                var creator = UserContext.Get().FirstOrDefault(u => u.Id == creatorRelation.User);
                if (creator == null) continue;

                var projectCard = new Main.ProjectCard
                {
                    ProjectNumber = project.Id,
                    ProjectName = project.Name,
                    CreatorName = creator.FullName,
                    UserRole = projUser.Role
                };

                projectCard.ProjectClicked += (sender, e) =>
                {
                    
                };

                ProjectsPanel.Children.Add(projectCard);
            }
        }

        private void Bt_CreateProject(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new creatProject());
        }

        private void Bt1_Projects(object sender, RoutedEventArgs e)
        {
            LoadProjects();
        }

        private void PAText_MouseDown(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
    }
}