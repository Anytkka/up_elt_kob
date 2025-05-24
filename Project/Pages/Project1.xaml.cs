using Project.Classes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            var allProjects = ProjectContext.Get();
            var userProjectRelations = ProjUserContext.GetByUserId(currentUser.Id);

            var visibleProjects = allProjects
                .Where(p => p.IsPublic || userProjectRelations.Any(upr => upr.Project == p.Id))
                .ToList();

            if (visibleProjects.Count == 0)
            {
                var noProjectsText = new TextBlock
                {
                    Text = "У вас пока нет доступных проектов",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16
                };
                ProjectsPanel.Children.Add(noProjectsText);
                return;
            }

            foreach (var project in visibleProjects)
            {
                var creatorRelation = ProjUserContext.GetByProjectId(project.Id)
                    .FirstOrDefault(pu => pu.Role == "Создатель");

                if (creatorRelation == null) continue;

                var creator = UserContext.Get().FirstOrDefault(u => u.Id == creatorRelation.User);
                if (creator == null) continue;

                var userRole = userProjectRelations
                    .FirstOrDefault(upr => upr.Project == project.Id)?.Role ?? "Не участвует";

                var projectCard = new Main.ProjectCard
                {
                    ProjectNumber = project.Id,
                    ProjectName = project.Name,
                    CreatorName = creator.FullName,
                    UserRole = userRole,
                    IsPublic = project.IsPublic
                };

                projectCard.ProjectClicked += (sender, projectId) =>
                {
                    // Все роли могут просматривать проект (переходить на страницу Kanban)
                    NavigationService?.Navigate(new Kanban(projectId));
                };

                // Добавляем кнопку удаления только для создателя
                if (userRole == "Создатель")
                {
                    projectCard.DeleteProjectClicked += (sender, projectId) =>
                    {
                        if (MessageBox.Show("Вы уверены, что хотите удалить проект?", "Подтверждение",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            var projectToDelete = allProjects.FirstOrDefault(p => p.Id == projectId);
                            if (projectToDelete != null)
                            {
                                projectToDelete.Delete();
                                LoadProjects();
                            }
                        }
                    };
                }

                ProjectsPanel.Children.Add(projectCard);
            }
        }

        private void Bt_CreateProject(object sender, RoutedEventArgs e)
        {
            var currentUser = App.CurrentUser;
            if (currentUser == null) return;

            // Проверяем, есть ли у пользователя роль "Создатель" в каком-либо проекте
            var userProjects = ProjUserContext.GetByUserId(currentUser.Id);
            bool isCreator = userProjects.Any(upr => upr.Role == "Создатель");

            if (!isCreator)
            {
                MessageBox.Show("Только пользователь с ролью 'Создатель' может создавать проекты.",
                                "Ошибка доступа",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            NavigationService?.Navigate(new creatProject());
        }

        private void Bt1_Projects(object sender, RoutedEventArgs e)
        {
            LoadProjects();
        }

        private void PAText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new PersonalAccount());
        }
    }
}