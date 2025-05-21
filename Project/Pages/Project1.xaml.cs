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
            // Очищаем панель проектов
            ProjectsPanel.Children.Clear();

            // Получаем текущего пользователя
            var currentUser = App.CurrentUser;
            if (currentUser == null) return;

            // Получаем все проекты из базы
            var allProjects = ProjectContext.Get();

            // Получаем все связи пользователя с проектами
            var userProjectRelations = ProjUserContext.GetByUserId(currentUser.Id);

            // Фильтруем проекты:
            // 1. Все открытые проекты (IsPublic = true)
            // 2. Закрытые проекты, где пользователь добавлен (любая роль)
            var visibleProjects = allProjects
                .Where(p => p.IsPublic ||
                           userProjectRelations.Any(upr => upr.Project == p.Id))
                .ToList();

            if (visibleProjects.Count == 0)
            {
                // Если нет проектов, показываем сообщение
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

            // Для каждого видимого проекта создаем карточку
            foreach (var project in visibleProjects)
            {
                // Получаем создателя проекта
                var creatorRelation = ProjUserContext.GetByProjectId(project.Id)
                    .FirstOrDefault(pu => pu.Role == "Создатель");

                if (creatorRelation == null) continue;

                var creator = UserContext.Get().FirstOrDefault(u => u.Id == creatorRelation.User);
                if (creator == null) continue;

                // Получаем роль текущего пользователя в проекте (если есть)
                var userRole = userProjectRelations
                    .FirstOrDefault(upr => upr.Project == project.Id)?.Role ?? "Не участвует";

                // Создаем карточку проекта
                var projectCard = new Main.ProjectCard
                {
                    ProjectNumber = project.Id,
                    ProjectName = project.Name,
                    CreatorName = creator.FullName,
                    UserRole = userRole,
                    IsPublic = project.IsPublic
                };

                // Добавляем обработчик клика
                projectCard.ProjectClicked += (sender, e) =>
                {
                    // Здесь можно добавить логику перехода к задачам проекта
                    MessageBox.Show($"Переход к проекту {project.Name}. Ваша роль: {userRole}");
                };

                // Добавляем карточку в панель
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