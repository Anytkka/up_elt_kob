using Project.Classes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Project.Pages
{
    public partial class ViewProject : Page
    {
        private int _projectId;
        private Classes.Project _currentProject;

        public ViewProject(int projectId)
        {
            InitializeComponent();
            _projectId = projectId;
            LoadProjectData();
        }

        private void LoadProjectData()
        {
            _currentProject = ProjectContext.Get().FirstOrDefault(p => p.Id == _projectId);
            if (_currentProject == null)
            {
                MessageBox.Show("Проект не найден!");
                NavigationService?.GoBack();
                return;
            }

         
            NameProjectLabel.Content = _currentProject.Name;
            DescriptionLabel.Content = _currentProject.Description ?? "Описание отсутствует";
            PublicityLabel.Content = _currentProject.IsPublic ? "Публичный" : "Закрытый";

  
            var creatorRelation = ProjUserContext.GetByProjectId(_projectId)
                .FirstOrDefault(pu => pu.Role == "Создатель");

            if (creatorRelation != null)
            {
                var creator = UserContext.Get().FirstOrDefault(u => u.Id == creatorRelation.User);
                CreatorLabel.Content = creator != null ?
                    $"Создатель: {creator.FullName}" :
                    "Создатель: Неизвестен";
            }

            LoadProjectMembers();
        }

        private void LoadProjectMembers()
        {
            MembersListView.Items.Clear();

            
            var projectMembers = ProjUserContext.GetByProjectId(_projectId);
            var allUsers = UserContext.Get();

            foreach (var member in projectMembers)
            {
                var user = allUsers.FirstOrDefault(u => u.Id == member.User);
                if (user != null)
                {
                   
                    var memberInfo = $"{user.FullName} ({member.Role})";
                    MembersListView.Items.Add(new ListViewItem { Content = memberInfo });
                }
            }

            
            if (MembersListView.Items.Count == 0)
            {
                MembersListView.Items.Add(new ListViewItem
                {
                    Content = "В проекте нет участников",
                    Foreground = System.Windows.Media.Brushes.Gray
                });
            }
        }

        private void Bt8_Close(object sender, RoutedEventArgs e)
        {
            
            NavigationService?.GoBack();
        }
    }
}